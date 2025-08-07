using Godot;
using System;

namespace RuntimeConsole;

public partial class ArgsPanel : PopupPanel
{
    private VBoxContainer _argsList;
    private Button _invokeMethodButton;
    private int _argsCount;
    public event Action<object[]> MethodInvoked;

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
        _argsList = GetNode<VBoxContainer>("%ArgsList");
        _invokeMethodButton = GetNode<Button>("%InvokeMethod");

        _invokeMethodButton.Pressed += OnInvokeMethodPressed;
    }

    public void SetArgs(params Type[] argTypes)
    {
        _argsCount = argTypes.Length;
        foreach (var child in _argsList.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var argType in argTypes)
        {
            var editor = _argValueEditor.Instantiate<ArgValueEditor>();
            editor.SetArgInfo(argType);
            _argsList.AddChild(editor);
        }
    }

    private void OnInvokeMethodPressed()
    {
        var args = new object[_argsCount];
        for (int i = 0; i < _argsCount; i++)
        {
            var editor = _argsList.GetChild<ArgValueEditor>(i);
            args[i] = editor.GetValue();
        }

        MethodInvoked?.Invoke(args);
        
            
    }
}
