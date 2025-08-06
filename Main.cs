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
    List<Error> errors = [Error.Ok, Error.FileNotFound, Error.CantOpen];
    [ShowInInspector]
    System.Collections.Generic.Dictionary<string, Error> errorDict = new()
    {
        ["item1"] = Error.Ok,
        ["item2"] = Error.Busy,
        ["item3"] = Error.Bug
    };
    [ShowInInspector]
    Variant variant = 1;
    [ShowInInspector]
    Variant variant1 = 2;
    [ShowInInspector]
    Variant variant2 = "Hello";
    
    public override void _Ready()
    {
       


    }

}

