[<AutoOpen>]
module FSharp.Validations.StringRules

open System.Text.RegularExpressions

let notEmpty = rule ((<>) "") (Some "Value cannot be empty")
let notNull = rule<string> ((<>) null) (Some "Value cannot be null")
let matches pattern msg = rule<string> (fun value -> Regex.IsMatch(value, pattern)) (msg |> Option.orElse (Some $"Value does not match the pattern {pattern}"))
let minLength len = rule<string> (fun value -> value.Length >= len) (Some $"Value is too short. Must be less than {len}")
let maxLength len = rule<string> (fun value -> value.Length <= len) (Some $"Value is too long. Must be less than {len}")
let lengthBetween min max = rule<string> (fun value -> value.Length >= min && value.Length <= max) (Some $"Value must be between {min} and {max} characters long")
let email = matches @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$" (Some "Value is not a valid email address")
let url = matches @"^(https?://)?([\w\-]+)((\.(\w){2,3})+)(/.*)?$" (Some "Value is not a valid URL")
let phone = matches @"^\+?(\d{1,3})?[-. ]?\(?(\d{3})\)?[-. ]?(\d{3})[-. ]?(\d{4})$" (Some "Value is not a valid phone number")
let zipCode = matches @"^\d{5}(-\d{4})?$" (Some "Value is not a valid zip code")
let alpha = matches @"^[a-zA-Z]+$" (Some "Value must contain only letters")
let alphaNumeric = matches @"^[a-zA-Z0-9]+$" (Some "Value must contain only letters and numbers")
let numeric = matches @"^\d+$" (Some "Value must contain only numbers")
let decimal = matches @"^\d+(\.\d+)?$" (Some "Value must be a decimal number")
let hexadecimal = matches @"^0[xX][0-9a-fA-F]+$" (Some "Value must be a hexadecimal number")
let base64 = matches @"^([0-9a-zA-Z+/]{4})*([0-9a-zA-Z+/]{2}==|[0-9a-zA-Z+/]{3}=)?$" (Some "Value must be a base64 encoded string")
let guid = matches @"^(\{{0,1}[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\}{0,1})$" (Some "Value must be a GUID")
let ipv4 = matches @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$" (Some "Value must be an IPv4 address")
let ipv6 = matches @"^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,7}:$|^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|^[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})$|^:((:[0-9a-fA-F]{1,4}){1,7}|:)$" (Some "Value must be an IPv6 address")
let macAddress = matches @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$" (Some "Value must be a MAC address")
