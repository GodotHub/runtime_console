using Godot;
using System;

namespace RuntimeConsole;

public partial class ArgsPanel : PopupPanel
{
    private HBoxContainer _argsList;
    private Button _invokeMethodButton;

    private PackedScene _argValueEditor = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MethodEditor/ArgValueEditor.tscn");

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }

    private void OnSceneInstantiated()
    {
        _argsList = GetNode<HBoxContainer>("%ArgsList");
        _invokeMethodButton = GetNode<Button>("%InvokeMethod"); 
    }

}
