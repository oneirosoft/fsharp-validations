[<AutoOpen>]
module FSharp.Validations.Validator

open System
open System.Linq.Expressions
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter

type ValidationErr = { property: string; message: string }
type ValidationResult<'a> =
    | Success of 'a
    | Failure of Map<string, string list>

let private createExpression (expr: ('a -> 'b) Expr) =
    let eval (expr: Expression) =
        (expr :?> Expression<Func<'a, 'b>>).Compile()
    
    let lambda = 
        <@ Func<'a, 'b> %expr @>
        |> QuotationToLambdaExpression
        |> eval

    match expr with
    | Lambda(_, PropertyGet(_, prop, _)) ->
        (lambda, prop.Name)
    | _ -> failwith $"Ensure that the expression is a property access expression (fun foo -> foo.Bar). Found: {expr}"

let rule<'a> (predicate: 'a -> bool) (message: string option) =
    (predicate, message |> Option.defaultValue "Validation failed")

let ruleFor<'a, 'b> (selector: Expr<'a -> 'b>) (rules: (('b -> bool) * string) list) entity =
    
    let selector, propertyName = createExpression selector
    
    rules
    |> List.map (fun (rule, message) -> (entity |> selector.Invoke |> rule, message))
    |> List.map (function | true, _ -> Ok entity | _, msg -> Error { message = msg; property = propertyName })
    |> List.fold (fun acc x ->
        match (x, acc) with
        | Ok x, Ok _ -> Ok x
        | Error result, Error lst -> Error (result :: lst)
        | Error result, Ok _ -> Error [result]
        | Ok _, Error lst -> Error lst) (Ok entity)

let ruleSet<'a> rules entity =
    rules
    |> List.map (fun rule -> rule entity)
    |> List.fold (fun acc x ->
        match (x, acc) with
        | Ok x, Success _ -> Success x
        | Error problems, Success _ ->
            problems
            |> List.groupBy _.property
            |> List.map (fun (key, value) -> (key, value |> List.map _.message))
            |> Map.ofList
            |> Failure
        | Error problems, Failure map ->
            problems
            |> List.groupBy _.property
            |> List.map (fun (key, value) -> (key, value |> List.map _.message))
            |> List.fold (fun acc (key, values) ->
                Map.change key (function | Some x -> Some (x @ values) | _ -> Some values) acc) map
            |> Failure
        | Ok _, Failure map -> Failure map) (Success entity)