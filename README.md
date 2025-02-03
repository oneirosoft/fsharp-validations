# FSharp.Validations

FSharp Validations is a validation library designed with a functional first
approach.

The library supports:

- The ability to create rules for properties using the `RuleFor<'a, 'b>` function
- The ability to apply a list of rules to a propery using `Rule<'a>`
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

