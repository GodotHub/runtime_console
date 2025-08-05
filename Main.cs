using Godot;
using System;
using RuntimeConsole;
using System.Collections.Generic;
using System.Collections;

[ExtendedInspector]
public partial class Main : Control
{    
    [ShowInInspector]
    (string name, int value) myTuple = ("Tuple", 10);
    [ShowInInspector]
    [Export] Variant var { get; set; } = 10;
    [ShowInInspector]
    [Export] Variant var2 { get; set; }
    [ShowInInspector]
    [Export] Godot.Collections.Array godotArray = [1, "String", true];
    [ShowInInspector]
    [Export] Godot.Collections.Array<int> godotIntArray = [1, 2, 3];
    [ShowInInspector]
    [Export] Godot.Collections.Dictionary godotDict = new() {["Key"] = "Value", ["Key2"] = 10, ["Key3"] = true};
    [ShowInInspector]
    [Export] Godot.Collections.Dictionary<string, int> godotStaticDict = new() {["Key"] = 11, ["Key2"] = 10, ["Key3"] = 9};
    [ShowInInspector]
    [Export] int[] myArray = [1,2,3];
    
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
    [Export] Node MyNode = new();
    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            nodes.Add(child);
        }        

    }  
}
