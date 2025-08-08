using Godot;
using System;
using RuntimeConsole;
using System.Collections.Generic;
using System.Collections;
using Godot.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

[ExtendedInspector]
public partial class Main : Control
{
    [Signal]
    public delegate void TestSignalEventHandler(string message);
    
    private Action<string> _customEventField;
    
   
    [EventFieldName("_customEventField")]
    public event Action<string> CustomEvent
    {
        add
        {
            _customEventField += value;
        }
        remove
        {
            _customEventField -= value;
        }
    }
    public override void _Ready()
    {
        CustomEvent += OnCustomEvent;
        CustomEvent += GD.Print;
        TestSignal += GD.Print;        
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
             _customEventField?.Invoke("Custom Event Triggered!");
            EmitSignalTestSignal("114514");
        }
    }
    private void OnCustomEvent(string message)
    {
        GD.Print($"Received custom event with message: {message}");
    }
    public static void StaticMethod() => GD.Print("Static Method");
    public void HelloWorld()
    {
        GD.Print("Hello World!");
    }

    public void Hello()
    {
        GD.Print("Hello!");

    }

    public void World()
    {
        GD.Print("World!");    
    }
}