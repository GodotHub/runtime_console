class_name Main
extends Control

enum MyEnum {
    ONE,
    TWO,
    THREE
}

var my_value
var arr = [[1, 2, 3], [4, 5, 6]]
var errors :Array[Error]= [Error.OK, Error.FAILED, Error.ERR_CANT_OPEN]
@export var my_int_enum: Array[MyEnum]
@export_flags("ONE:2","TWO:4","THREE:6") var my_enum:Array[int]
var color_arr:Array[Color] = [Color.RED, Color.ALICE_BLUE]
var a = 10
var b = 3.14
var c = "Hello"
var d := true
var e : Array[int] = [1, 2, 3]
var f : Dictionary[String, Error] = {"one": Error.OK, "two": Error.FAILED}
var item = Item.new()
@export var error : Error
var variant

class Item extends Resource:
    @export var name = "Item"
    @export var id = 0     

func _ready():    
    print(my_enum)
    pass

func foo(a, b, c):
    pass

func add(a: int, b: int) -> int:
    return a + b