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
    Godot.Collections.Array<Error> godotErrors =[Error.Ok, Error.FileNotFound, Error.CantOpen];
    [ShowInInspector]
    List<PropertyUsageFlags> errors = [PropertyUsageFlags.Editor, PropertyUsageFlags.NoEditor];
    [ShowInInspector]
    System.Collections.Generic.Dictionary<Error, Error> errorDict = new()
    {
        [Error.AlreadyExists] = Error.Ok,
        [Error.AlreadyInUse] = Error.Busy,
        [Error.Bug] = Error.Bug
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

