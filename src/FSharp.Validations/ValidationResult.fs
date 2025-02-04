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
    
    let toResult = function
        | Success x -> Ok x
        | Failure x -> Error x
    
    let toOption = function
        | Success x -> Some x
        | Failure _ -> None
        
    let fromResult = function
        | Ok x -> Success x
        | Error x -> Failure x
        
    let fromOption = function
        | Some x -> Success x
        | None -> Failure Map.empty
        
    let toList = function
        | Success x -> [x]
        | Failure _ -> []
        
    let toArray = function
        | Success x -> [|x|]
        | Failure _ -> [||]
        
    let bind f = function
        | Success x -> f x
        | Failure x -> Failure x
    
    let map f = bind (fun x -> Success (f x)) 
        
    let filter f = function
        | Success x when f x -> Success x
        | Success _ -> Failure Map.empty
        | Failure x -> Failure x
    
    let defaultValue x = function
        | Success x -> x 
        | Failure _ -> x
        
    let defaultWith f = function
        | Success x -> x
        | Failure failures -> f failures
    
    let iter f = map f >> ignore 
        
    let orElse x = function
        | Success x -> Success x
        | Failure _ -> x
        
    let orElseWith f = function
        | Success x -> Success x
        | Failure failures -> f failures
    
    let fold (folder: 'State -> 'a -> 'State) state = function
        | Success x -> folder state x
        | Failure _ -> state
        
    let foldBack (folder: 'a -> 'State -> 'State) value state =
        match value with
        | Success x -> folder x state
        | Failure _ -> state
    
    let exists f = map f >> defaultValue false 
    
    let forall = exists
    
    let contains value = exists ((=) value) 
    
    let flatten = function
        | Success (Success x) -> Success x
        | Success (Failure x) -> Failure x
        | Failure x -> Failure x

    let count value = fold (fun acc _ -> acc + 1) 0 value