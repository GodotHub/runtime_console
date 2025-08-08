@warning_ignore_start("unused_parameter")
@warning_ignore_start("unused_signal")
class_name Main
extends Control

enum Test
{
	TEST_1,
	TEST_2,
	TEST_3
}
signal test_signal(a, b, c)
signal test_signal_2(a:Array[int], b:int, c:Dictionary[String, Test])
class Item:
	var a:int = 0
	var b:String = ""


func _ready():
	test_signal.connect(foo)
	test_signal_2.connect(foo)
	pass

func _process(delta):
	if Input.is_action_just_pressed("ui_accept"):
		emit_signal("test_signal", 1, 2, 3)
func sum(arr: Array[Error]) -> int:
	var sum:int = 0
	for i in arr:
		sum += i
	return sum

func foo(a, b, c):
	prints(a, b, c)

func add(a: int, b: int) -> int:
	return a + b
