using Godot;
using System;
using RuntimeConsole;
using System.Collections.Generic;
using System.Collections;
using Godot.Collections;
using System.Diagnostics;
using System.Reflection;

[ExtendedInspector]
public partial class Main : Control
{

    [ShowInInspector]
    (int Value, string Name) TestTuple = (10, "Test");
    [ShowInInspector]
    TestStruct testStruct = new() { Value = 10, Name = "Test", ints = [ 10, 20, 30 ] };
    [ShowInInspector]
    [Export] Variant var { get; set; } = 10;
    [ShowInInspector]
    [Export] Variant var2 { get; set; }
    [ShowInInspector]
    [Export] Godot.Collections.Array godotArray = [1, "String", true];
    [ShowInInspector]
    [Export] Godot.Collections.Array<int> godotIntArray = [1, 2, 3];
    [ShowInInspector]
    [Export] Godot.Collections.Dictionary godotDict = new() { ["Key"] = "Value", ["Key2"] = 10, ["Key3"] = true };
    [ShowInInspector]
    [Export] Godot.Collections.Dictionary<string, int> godotStaticDict = new() { ["Key"] = 11, ["Key2"] = 10, ["Key3"] = 9 };
    [ShowInInspector]
    [Export] int[] myArray = [1, 2, 3];

    [ShowInInspector]
    int[][] myArray2 = [[1, 2, 3], [1, 2, 3]];

    [ShowInInspector]
    int[,] myArray3 = { { 1, 2, 3 }, { 1, 2, 3 } };

    [ShowInInspector]
    List<Node> nodes = [];

    [ShowInInspector]
    List<int> ints = [1, 2, 3];

    [ShowInInspector]
    System.Collections.Generic.Dictionary<string, int> dict = new() { ["item1"] = 1, ["item2"] = 2, ["item3"] = 3 };
    [ShowInInspector]
    [Export] Node MyNode = new();

    [ShowInInspector]
    Hashtable hashtable = new() { ["item1"] = 1, ["item2"] = 2, ["item3"] = 3 };
    [ShowInInspector]
    Stack<int> stack = new();
    [ShowInInspector]
    Queue<int> queue = new();
    [ShowInInspector]
    HashSet<int> hashset = [1, 2, 3, 4];
    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            nodes.Add(child);
        }

        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Push(4);

        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);


    }

    struct TestStruct
    {
        public int Value;
        public string Name;
        public int[] ints;
    }
}

