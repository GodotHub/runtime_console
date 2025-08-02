# Implement IConsoleCommand 
extends Resource

var keyword: String:
	get:
		return "" # Replace with your command keyword

var parameter_types: Array[Variant.Type]:
	get:
		return [] # Replace with the expected parameter types (in order)

func execute(args: Array) -> void:
	pass # Replace with your command logic