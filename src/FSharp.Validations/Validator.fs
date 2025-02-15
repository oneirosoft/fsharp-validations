﻿[<AutoOpen>]
module FSharp.Validations.Validator

open System
open System.Linq.Expressions
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter

type ValidationError private = { property: string; message: string }

type Validator<'a> = 'a -> 'a ValidationResult

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
    |> List.map (function
        | true, _ -> Ok entity
        | _, msg -> Error { message = msg; property = propertyName })
    |> List.fold (fun acc x ->
        match (x, acc) with
        | Ok x, Success _ -> Success x
        | Error problem, Success _ ->
            [ problem.property, [ problem.message ] ]
            |> Map.ofList
            |> Failure
        | Error problem, Failure map ->
           Map.change problem.property (Option.map ((@) [ problem.message ]) >> Option.orElse (Some [ problem.message ])) map
           |> Failure
        | Ok _, Failure map -> Failure map
    ) (Success entity) 
    
let ruleSetFor<'a, 'b> (selector: Expr<'a -> 'b>) (validator: 'b Validator) entity =
    let selector, propertyName = createExpression selector
    
    match entity |> selector.Invoke |> validator with
    | Success _ -> Success entity
    | Failure map ->
        map
        |> Map.toList
        |> List.map (fun (key, value) -> ($"{propertyName}.{key}", value))
        |> Map.ofList
        |> Failure
        
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
let ruleSet<'a> (rules: list<'a -> 'a ValidationResult>): 'a Validator =
    fun entity ->
    rules
    |> List.map (fun rule -> rule entity)
    |> List.fold (fun acc x ->
        match (x, acc) with
        | Success _, Success _ -> Success entity
        | Failure map, Success _ -> Failure map
        | Failure map, Failure map' ->
            map |> Map.fold (fun acc key value -> acc |> Map.add key value) map'
            |> Failure
        | Success _, Failure map -> Failure map
    ) (Success entity)
    