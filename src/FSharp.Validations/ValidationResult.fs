namespace FSharp.Validations

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

module ValidationResult =
    
    /// <summary>
    /// Converts a ValidationResult to a Result type.
    /// </summary>
    /// <returns>
    /// Returns `Ok` with the validated entity if the ValidationResult is a success.
    /// Returns `Error` with the map of property names to error messages if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.toResult (Success "Valid")
    /// // result is Ok "Valid"
    /// </code>
    /// </example>
    let toResult = function
        | Success x -> Ok x
        | Failure x -> Error x

    /// <summary>
    /// Converts a ValidationResult to an Option type.
    /// </summary>
    /// <returns>
    /// Returns `Some` with the validated entity if the ValidationResult is a success.
    /// Returns `None` if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let option = ValidationResult.toOption (Success "Valid")
    /// // option is Some "Valid"
    /// </code>
    /// </example>
    let toOption = function
        | Success x -> Some x
        | Failure _ -> None

    /// <summary>
    /// Converts a Result type to a ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns `Success` with the validated entity if the Result is `Ok`.
    /// Returns `Failure` with the map of property names to error messages if the Result is `Error`.
    /// </returns>
    /// <example>
    /// <code>
    /// let validationResult = ValidationResult.fromResult (Ok "Valid")
    /// // validationResult is Success "Valid"
    /// </code>
    /// </example>
    let fromResult = function
        | Ok x -> Success x
        | Error x -> Failure x

    /// <summary>
    /// Converts an Option type to a ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns `Success` with the validated entity if the Option is `Some`.
    /// Returns `Failure` with an empty map if the Option is `None`.
    /// </returns>
    /// <example>
    /// <code>
    /// let validationResult = ValidationResult.fromOption (Some "Valid")
    /// // validationResult is Success "Valid"
    /// </code>
    /// </example>
    let fromOption = function
        | Some x -> Success x
        | None -> Failure Map.empty

    /// <summary>
    /// Converts a ValidationResult to a list.
    /// </summary>
    /// <returns>
    /// Returns a list containing the validated entity if the ValidationResult is a success.
    /// Returns an empty list if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let list = ValidationResult.toList (Success "Valid")
    /// // list is ["Valid"]
    /// </code>
    /// </example>
    let toList = function
        | Success x -> [x]
        | Failure _ -> []

    /// <summary>
    /// Converts a ValidationResult to an array.
    /// </summary>
    /// <returns>
    /// Returns an array containing the validated entity if the ValidationResult is a success.
    /// Returns an empty array if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let array = ValidationResult.toArray (Success "Valid")
    /// // array is [|"Valid"|]
    /// </code>
    /// </example>
    let toArray = function
        | Success x -> [|x|]
        | Failure _ -> [||]
        
    /// <summary>
    /// Binds a function to a ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns the result of applying the function to the validated entity if the ValidationResult is a success.
    /// Returns the original failure if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.bind (fun x -> Success (x + 1)) (Success 1)
    /// // result is Success 2
    /// </code>
    /// </example>
    let bind f = function
        | Success x -> f x
        | Failure x -> Failure x

    /// <summary>
    /// Maps a function over a ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns a new ValidationResult with the function applied to the validated entity if the ValidationResult is a success.
    /// Returns the original failure if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.map ((+) 1) (Success 1)
    /// // result is Success 2
    /// </code>
    /// </example>
    let map f = bind (fun x -> Success (f x))

    /// <summary>
    /// Filters a ValidationResult based on a predicate.
    /// </summary>
    /// <returns>
    /// Returns the original ValidationResult if the predicate is true for the validated entity.
    /// Returns a failure with an empty map if the predicate is false for the validated entity.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.filter ((=) 1) (Success 1)
    /// // result is Success 1
    /// </code>
    /// </example>
    let filter f = function
        | Success x when f x -> Success x
        | Success _ -> Failure Map.empty
        | Failure x -> Failure x

    /// <summary>
    /// Returns the validated entity or a default value.
    /// </summary>
    /// <returns>
    /// Returns the validated entity if the ValidationResult is a success.
    /// Returns the default value if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let value = ValidationResult.defaultValue 0 (Success 1)
    /// // value is 1
    /// </code>
    /// </example>
    let defaultValue x = function
        | Success x -> x
        | Failure _ -> x

    /// <summary>
    /// Returns the validated entity or a value generated by a function.
    /// </summary>
    /// <returns>
    /// Returns the validated entity if the ValidationResult is a success.
    /// Returns the value generated by the function if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let value = ValidationResult.defaultWith (fun _ -> 0) (Failure Map.empty)
    /// // value is 0
    /// </code>
    /// </example>
    let defaultWith f = function
        | Success x -> x
        | Failure failures -> f failures

    /// <summary>
    /// Applies a function to the validated entity if the ValidationResult is a success.
    /// </summary>
    /// <example>
    /// <code>
    /// ValidationResult.iter (printfn "%d") (Success 1)
    /// // prints "1"
    /// </code>
    /// </example>
    let iter f = map f >> ignore

    /// <summary>
    /// Returns the original ValidationResult or another ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns the original ValidationResult if it is a success.
    /// Returns the other ValidationResult if the original is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.orElse (Success 2) (Failure Map.empty)
    /// // result is Success 2
    /// </code>
    /// </example>
    let orElse x = function
        | Success x -> Success x
        | Failure _ -> x

    /// <summary>
    /// Returns the original ValidationResult or a ValidationResult generated by a function.
    /// </summary>
    /// <returns>
    /// Returns the original ValidationResult if it is a success.
    /// Returns the ValidationResult generated by the function if the original is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.orElseWith (fun _ -> Success 2) (Failure Map.empty)
    /// // result is Success 2
    /// </code>
    /// </example>
    let orElseWith f = function
        | Success x -> Success x
        | Failure failures -> f failures

    /// <summary>
    /// Folds a function over a ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns the state after applying the function to the validated entity if the ValidationResult is a success.
    /// Returns the original state if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let state = ValidationResult.fold (fun acc x -> acc + x) 0 (Success 1)
    /// // state is 1
    /// </code>
    /// </example>
    let fold (folder: 'State -> 'a -> 'State) state = function
        | Success x -> folder state x
        | Failure _ -> state

    /// <summary>
    /// Folds a function over a ValidationResult from the back.
    /// </summary>
    /// <returns>
    /// Returns the state after applying the function to the validated entity if the ValidationResult is a success.
    /// Returns the original state if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let state = ValidationResult.foldBack (fun x acc -> acc + x) (Success 1) 0
    /// // state is 1
    /// </code>
    /// </example>
    let foldBack (folder: 'a -> 'State -> 'State) value state =
        match value with
        | Success x -> folder x state
        | Failure _ -> state

    /// <summary>
    /// Checks if a predicate holds for the validated entity.
    /// </summary>
    /// <returns>
    /// Returns true if the predicate holds for the validated entity.
    /// Returns false if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let exists = ValidationResult.exists ((=) 1) (Success 1)
    /// // exists is true
    /// </code>
    /// </example>
    let exists f = map f >> defaultValue false

    /// <summary>
    /// Checks if a predicate holds for all validated entities.
    /// </summary>
    /// <returns>
    /// Returns true if the predicate holds for all validated entities.
    /// Returns false if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let forall = ValidationResult.forall ((=) 1) (Success 1)
    /// // forall is true
    /// </code>
    /// </example>
    let forall = exists

    /// <summary>
    /// Checks if the validated entity contains a value.
    /// </summary>
    /// <returns>
    /// Returns true if the validated entity contains the value.
    /// Returns false if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let contains = ValidationResult.contains 1 (Success 1)
    /// // contains is true
    /// </code>
    /// </example>
    let contains value = exists ((=) value)

    /// <summary>
    /// Flattens a nested ValidationResult.
    /// </summary>
    /// <returns>
    /// Returns the inner ValidationResult if the outer ValidationResult is a success.
    /// Returns the original failure if the outer ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let result = ValidationResult.flatten (Success (Success 1))
    /// // result is Success 1
    /// </code>
    /// </example>
    let flatten = function
        | Success (Success x) -> Success x
        | Success (Failure x) -> Failure x
        | Failure x -> Failure x

    /// <summary>
    /// Counts the number of validated entities.
    /// </summary>
    /// <returns>
    /// Returns the count of validated entities if the ValidationResult is a success.
    /// Returns 0 if the ValidationResult is a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// let count = ValidationResult.count (Success 1)
    /// // count is 1
    /// </code>
    /// </example>
    let count value = fold (fun acc _ -> acc + 1) 0 value 