[<AutoOpen>]
module FSharp.Validations.Validator

open System
open System.Linq.Expressions
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter

type ValidationError private = { property: string; message: string }

/// <summary>
/// <para>
/// Validation result union which can be either a success or a failure.
/// </para>
/// <para>
/// A success contains the validated entity.
/// </para>
/// <para>
/// A failure contains a map of property names to error messages.
/// </para>
/// </summary>
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

/// <summary>
/// An individual rule for a given property.
///
/// <code>
/// let notEmpty = rule ((&lt;>) "") (Some "Bar cannot be empty")
/// let notNull = rule&lt;string&gt; ((&lt;>) null) (Some "Bar cannot be null")
/// </code>
/// <param name="predicate">
/// A predicate function that returns true if the rule passes.
/// </param>
/// <para name="message">
/// An optional message to return if the rule fails.
/// </para>
/// <returns>
/// A tuple of the predicate and the message.
/// </returns>
/// </summary>
let rule<'a> (predicate: 'a -> bool) (message: string option) =
    (predicate, message |> Option.defaultValue "Validation failed")

/// <summary>
/// Creates a rule for the given entity's property.
/// <code>
/// type Foo = { Bar: string }
///
/// 
/// let barRule = ruleFor &lt;@ _.Bar @&gt; [ rule ((&lt;>) "") (Some "Bar cannot be empty") ]
/// </code>
/// <param name="selector">
/// The property selector expression
/// </param>
/// <para name="rules">
/// The individual rules to apply to the property.
/// </para>
/// <param name="entity">
/// The entity to evaluate.
/// </param>
/// <returns>
/// A Result of the entity or a list of validation errors.
/// If the result is Ok the entity is returned.
/// If the result is Error a list of errors is returned.
/// </returns>
/// </summary>
let ruleFor<'a, 'b> (selector: Expr<'a -> 'b>) (rules: (('b -> bool) * string) list) entity =
    
    let selector, propertyName = createExpression selector
    
    rules
    |> List.map (fun (rule, message) -> (entity |> selector.Invoke |> rule, message))
    |> List.map (function | true, _ -> Ok entity | _, msg -> Error { message = msg; property = propertyName })
    |> List.fold (fun acc x ->
        match (x, acc) with
        | Ok x, Ok _ -> Ok x
        | Error result, Error lst -> Error (lst @ [result])
        | Error result, Ok _ -> Error [result]
        | Ok _, Error lst -> Error lst) (Ok entity)

/// <summary>
/// Creates a rule set for the given entity.
/// <code>
/// 
/// type Foo = { Bar: string }
///
/// 
/// let validateFoo = ruleSet [
///     RuleFor &lt;@ _.Bar @> [ rule ((&lt;>) "") (Some "Bar cannot be empty") ] ]
/// </code>
/// <param name="rules">
/// A list of the property rules to apply to the entity. 
/// </param>
/// <param name="entity">
/// The entity to evaluate.
/// </param>
/// <returns>
/// A validation result.
/// If the result is a success, the entity is returned.
/// If the result is a failure, a map of property names to error messages is returned.
/// <see cref="ValidationResult{T}" />
/// </returns>
/// </summary>
let ruleSet<'a> rules (entity: 'a) =
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
                Map.change key (Option.map ((@) values) >> Option.orElse (Some values)) acc) map
            |> Failure
        | Ok _, Failure map -> Failure map) (Success entity)