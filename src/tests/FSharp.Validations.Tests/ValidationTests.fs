module ValidationTests

open FSharp.Validations
open Xunit

type Foo = { Bar: string }
type FooBar = { Bar: string; Baz: string }

let rules = [|
    [| rule ((<>) "") (Some "Value cannot be empty") :> obj; Some "Value cannot be empty" |]
    [| rule ((<>) "") None; None |]
|]

let data =
    [| [| { Bar = "Hello, World!" } :> obj |] |]

[<Theory>]
[<MemberData(nameof rules)>]
let ``rule has message`` (arg: (string -> bool) * string, expected: string option) =
    let fn, actualMsg = arg
    Assert.True(fn "foo")
    match expected with
    | Some expected -> Assert.Equal(expected, actualMsg)
    | None -> Assert.Equal("Validation failed", actualMsg)

[<Fact>]
let ``ruleFor produces ok`` () =
    let foo = { Bar = "Hello, World!" }
    let result = foo |> ruleFor <@ _.Bar @> [ notEmpty ]
    Assert.True(result.IsOk)
    
[<Fact>]
let ``ruleFor produces error`` () =
    let foo = { Bar = "" }
    let result = foo |> ruleFor <@ _.Bar @> [ notEmpty ]
    Assert.True(result.IsError)
    
[<Fact>]
let ``ruleFor produces multiple errors`` () =
    let foo = { Bar = "" }
    let result = foo |> ruleFor <@ _.Bar @> [ notEmpty; minLength 5 ]
    
    Assert.True(result.IsError)
    
    result
    |> Result.mapError _.Length
    |> Result.mapError (fun x -> Assert.Equal(2, x))
    
[<Fact>]
let ``ruleSet produces Success`` () =
    let foo = { Bar = "Hello, World!" }
    let result = foo |> ruleSet [ ruleFor <@ _.Bar @> [ notEmpty ] ]
    Assert.True(result.IsSuccess)
    
[<Fact>]
let ``ruleSet produces Failure`` () =
    let foo = { Bar = "" }
    let result = foo |> ruleSet [ ruleFor <@ _.Bar @> [ notEmpty ] ]
    Assert.True(result.IsFailure)
    
[<Fact>]
let ``ruleSet produces property failures`` () =
    let foo = { Bar = "" }
    let result =
         foo |>
         ruleSet [
             ruleFor <@ _.Bar @> [ notEmpty; minLength 5 ]
         ]
    
    match result with
    | Success _ -> Assert.Fail "Expected failure"
    | Failure fails ->
        let expected = fails |> Map.find "Bar" 
        Assert.Equal(expected.Length, 2)
    
    Assert.True(result.IsFailure)

[<Fact>]
let ``ruleSet produces multiple property failures`` () =
    let fooBar = { Bar = ""; Baz = "" }
    let validator =
        ruleSet [
            ruleFor <@ _.Bar @> [ notEmpty; minLength 5 ]
            ruleFor <@ _.Baz @> [ notEmpty; minLength 5 ]
        ]
    
    let result = fooBar |> validator
    match result with
    | Success _ -> Assert.Fail "Expected failure"
    | Failure fails ->
        let expectedBar = fails |> Map.find "Bar" 
        let expectedBaz = fails |> Map.find "Baz" 
        Assert.Equal(expectedBar.Length, 2)
        Assert.Equal(expectedBaz.Length, 2)
        
[<Fact>]
let ``ruleSet to IValidator and is valid`` () =
    let foo = { Bar = "Hello, World!" }
    let validator =
        ruleSet<Foo> [
            ruleFor <@ _.Bar @> [ notEmpty ]
        ]
        |> toValidator
    let result = foo |> validator.Validate
    Assert.True(result.IsSuccess)