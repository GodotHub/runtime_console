using Godot;
using System;

namespace RuntimeConsole;

public partial class ArgsPanel : PopupPanel
{
    private VBoxContainer _argsList;
    private Button _invokeMethodButton;
    private int _argsCount;
    private int _genericArgsCont;
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

    public void SetArgs(int genericArgsCount, params Type[] argTypes)
    {
        _argsCount = argTypes.Length;
        _genericArgsCont = genericArgsCount;
        foreach (var child in _argsList.GetChildren())
        {
            child.QueueFree();
        }

        for (int i = 0; i < genericArgsCount; i++)
        {
            var editor = _argValueEditor.Instantiate<ArgValueEditor>();
            editor.SetArgInfo(typeof(Type));
            editor.IsGenericArg = true;
            
            _argsList.AddChild(editor);
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
        var args = new object[_argsCount + _genericArgsCont];
        for (int i = 0; i < _argsCount + _genericArgsCont; i++)
        {
            var editor = _argsList.GetChild<ArgValueEditor>(i);
            args[i] = editor.GetValue();
        }

        MethodInvoked?.Invoke(args);
        
            
    }
}
