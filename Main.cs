using Godot;
using System;
using RuntimeConsole;

[ExtendedInspector]
public partial class Main : Control
{
    [ShowInInspector("变量")]
    public static int MyStaticVar { get; set; } = 0;
    public int MyVar2 { get; set; } = 0;

    private int MyPrivateVar { get; set; } = 0;

    [ShowInInspector("My Private Var")]
    private static int MyStaticPrivateVar { get; set; } = 0;

    public override void _Ready()
    {
        var node = GD.Load<PackedScene>("res://test.tscn").Instantiate<Test>();
        GD.Print(node.label);
    }
}
