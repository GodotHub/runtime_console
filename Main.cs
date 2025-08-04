using Godot;
using System;
using RuntimeConsole;
using System.Collections.Generic;

[ExtendedInspector]
public partial class Main : Control
{
    [Export] public Vector2 vector2;
    [Export] public Vector3 vector3;
    [Export] public Vector2I vector2I;
    [Export] public Vector3I vector3I;
    [Export] public Rect2 rect2;
    [Export] public Rect2I rect2I;
    [Export] public Quaternion quaternion;
    [Export] public Vector4 vector4;
    [Export] public Vector4I vector4I;
    [Export] public Aabb aabb;
    [Export] public Plane plane;
    [Export] public Transform2D transform2D;
    [Export] public Transform3D transform;
    [Export] public Basis basis;
    [ShowInInspector]
    List<Node> nodes { get; set; } = [];
    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            nodes.Add(child);
        }
    }  
}
