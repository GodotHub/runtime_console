using Godot;
using System;
using RuntimeConsole;
using System.Collections.Generic;

[ExtendedInspector]
public partial class Main : Control
{
    [ShowInInspector]
    Godot.Collections.Array godotArrat = [1, "String", true];
    [ShowInInspector]
    Godot.Collections.Dictionary godotDict = new() {["Key"] = "Value", ["Key2"] = 10, ["Key3"] = true};
    [ShowInInspector]
    int[] myArray = [1,2,3];
    
    [ShowInInspector]
    int[][] myArray2 = [[1,2,3],[1,2,3]];

    [ShowInInspector]
    int[,] myArray3 = { {1,2,3},{1,2,3} };

    [ShowInInspector]
    List<Node> nodes = [];

    [ShowInInspector]
    List<int> ints = [1,2,3];

    [ShowInInspector]
    Dictionary<string, int> dict = new() { ["item1"] = 1, ["item2"] = 2, ["item3"] = 3};
    [ShowInInspector]
    Node MyNode = new();
    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            nodes.Add(child);
        }

    }  
}
