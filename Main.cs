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

    public override void _Ready()
    {


    }
    public void Print(Variant variant) => GD.Print(variant);
    public void Hello()
    {
        GD.Print("Hello World!");
    
    }
}

