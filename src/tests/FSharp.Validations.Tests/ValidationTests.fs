module ValidationTests

open FSharp.Validations
open Xunit

type Foo = { Bar: string }
type Bar = { Foo: Foo; Baz: string }
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
    Assert.True(result.IsSuccess)
    
[<Fact>]
let ``ruleFor produces error`` () =
    let foo = { Bar = "" }
    let result = foo |> ruleFor <@ _.Bar @> [ notEmpty ]
    Assert.True(result.IsFailure)
    
[<Fact>]
let ``ruleFor produces multiple errors`` () =
    let foo = { Bar = "" }
    match foo |> ruleFor <@ _.Bar @> [ notEmpty; minLength 5; ] with
    | Success _ -> Assert.Fail "Expected failure"
    | Failure fails ->
        Assert.Equal (2, fails |> Map.find "Bar" |> Seq.length)
    
[<Fact>]
let ``ruleSet produces Success`` () =
    let foo = { Bar = "Hello, World!" }
    foo
    |> ruleSet [ ruleFor <@ _.Bar @> [ notEmpty ] ]
    |> ValidationResult.map (fun x -> Assert.Equal(foo, x))
    |> ValidationResult.defaultWith (fun _ -> Assert.Fail "Expected success")
    
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
let ``ruleSet with validatorFor and rule`` () =
    let foo = { Bar = "Hello, World!" }
    let bar = { Foo = foo; Baz = "" }
   
    let fooValidator =
        ruleSet<Foo> [ ruleFor <@ _.Bar @> [ maxLength 5 ] ]
        
    let barValidator =
        ruleSet<Bar> [
            ruleSetFor <@ _.Foo @> fooValidator
            ruleFor <@ _.Baz @> [ notEmpty ]
        ]
        
    let result = bar |> barValidator
    
    match result with
    | Success _ -> Assert.Fail "Expected failure"
    | Failure fails ->
        let expected = fails |> Map.find "Baz" 
        Assert.Equal(expected.Length, 1)
        let expected = fails |> Map.find "Foo.Bar"
        Assert.Equal(expected.Length, 1)
        
    
[<Fact>]
let ``ruleSet to IValidator and is valid`` () =
    let foo = { Bar = "Hello, World!" }
    let validator =
        ruleSet<Foo> [
            ruleFor <@ _.Bar @> [ notEmpty ]
        ]
        |> toValidator
    let result = foo |> validator.Validate
    match result with
    | Success x -> Assert.Equal(foo, x)
    | Failure _ -> Assert.Fail "Expected success"