namespace FSharp.Validations

type IValidator<'a> =
    abstract member Validate: 'a -> 'a ValidationResult
    
type private DefaultValidator<'a>(validator: 'a Validator) =
    interface IValidator<'a> with
        member _.Validate entity = validator entity

[<AutoOpen>]
module IValidator =
    /// <summary>
    /// Creates a validator from a validation function.
    /// <code>
    /// type Foo = { Bar: string }
    ///
    ///
    /// let fooValidator =
    ///     ruleSet [
    ///         RuleFor &lt;@ _.Bar @> [ notEmpty ] ]
    ///     |> toValidator
    /// </code>
    /// </summary>
    /// <param name="validator">
    /// The validation function to convert to an <see cref="IValidator{T}"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IValidator{T}"/> instance.
    /// </returns>
    let toValidator<'a> validator = DefaultValidator(validator) :> 'a IValidator