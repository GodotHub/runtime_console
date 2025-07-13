# Implement IParameterParser
extends Resource

var supported_types : Array[Variant.Type]:
    get:
        return [] # Replace with the types supported by this parser

var result : Variant:
    get:
        return _result

var _result : Variant

func parse(token: String) -> Error:
    # Replace with your parsing logic
    return OK