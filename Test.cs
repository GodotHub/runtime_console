using Godot;
using System;

public partial class Test : Node2D
{
    public Label label;
    public Test()
    {
        // GD.Print(label);
    }
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            // GD.Print(label);
            label = GetNode<Label>("Label");
        }
    }


}
