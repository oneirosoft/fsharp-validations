# FSharp.Validations

[![FSharp Validations CI](https://github.com/oneirosoft/fsharp-validations/actions/workflows/fsharp-validations.yml/badge.svg)](https://github.com/oneirosoft/fsharp-validations/actions/workflows/fsharp-validations.yml)
![NuGet Version](https://img.shields.io/nuget/v/Oneiro.FSharp.Validations?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FOneiro.FSharp.Validations)

FSharp Validations is a validation library designed with a functional first
approach.

The library supports:

- The ability to create rules for properties using the `RuleFor<'a, 'b>` function
- The ability to apply a list of rules to a propery using `Rule<'a>`
- the ability to reuse validators using `ruleSetFor<'a, 'b>`
- The creation of `RuleSets<'a> -> 'a ValidationResult`

The rules can then be evaluated at any time thanks to the magic of currying.

```fsharp
open FSharp.Validations

type Foo = { Bar: string }

let validateFoo =
    ruleSet [
        ruleFor <@ _.Bar @> [
            rule (fun x -> x.Length > 0) (Some "Length must be greater than 0")
            rule (fun x -> x.Length < 10) (Some "Length must be less than 10") ] ] 

let result = validateFoo { Bar = "Hello, World!" }

// result is Failure (map [("Bar", ["Value must be less than 10"])])
```

## Reusing Rule Sets

Rule sets can be reused by creating a `RuleSet<'a>` and applying it to
a property using the `ruleSetFor<'a, 'b>` function.

```fsharp
type Bar = { Value: string }
type Foo = { Bar: Bar }

let validateBar =
    ruleSet [
        ruleFor <@ _.Value @> [
            rule (fun x -> x.Length > 0) (Some "Length must be greater than 0")
            rule (fun x -> x.Length < 10) (Some "Length must be less than 10") ] ]

let validateFoo =
    ruleSet [
        ruleSetFor <@ _.Bar @> validateBar ]
```

_Any resulting errors for `Bar` will have a key of `Bar.{propertyName}`._

## `ValidationResult<'a>`

A `Success` represents the entity that has passed validations.

A `Failure` represents a `Map<string, string list>` where the key is
the property that failed and value is a collection of all the messages
attached to the failure upon validation.

```fsharp
let result = validateFoo { bar = "Hello, World!" }

match result with
| Success value -> printfn "%A" value
| Failure errors -> errors |> Map.iter (printfn "%s: %A")
```

## Dependency Injection

A function is provided to convert any `'a -> 'a ValidationResult` to
an `'a IValidator` using the provided `toValidator` function.

The resulting `'a IValidator` can be used for Dependency Injection.

```fsharp
open Microsoft.Extensions.DependencyInjection

type Foo = { Bar: string }

let validateFoo =
    ruleSet<Foo> [ ruleFor <@ _.Bar @> [ notEmpty ] ]

let fooValidator = validateFoo |> toValidator

let result = fooValidator.Validate({ Bar = "Hello, World!" })

// Add to Dependency Injection

let services = ServiceCollection() :> IServiceCollection
services.AddScoped<Foo IValidator>(fun _ -> fooValidator) |> ignore

```

## License, Copywright, Etc.

FSharp.Validations is subject to copywright @ 2025 Oneirosoft and other
contributors under the [MIT License](LICENSE). 
