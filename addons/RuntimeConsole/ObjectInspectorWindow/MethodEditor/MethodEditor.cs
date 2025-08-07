using Godot;
using System;

namespace RuntimeConsole;

public partial class MethodEditor : PanelContainer, IMemberEditor
{
    public event Action<object[]> Invoke;
    public string MemberName { get; private set; }

    public MemberEditorType MemberType => MemberEditorType.Method;
    public int ArgsCount { get; private set; }

    public Control AsControl() => this;
    private Label _signatureLabel;
    private Button _invokeButton;
    private PopupPanel _argsPanel = null;
    private static readonly PackedScene _argsPanelScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MethodEditor/ArgsPanel.tscn");

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }

    public void SetMethodInfo(string name, string signature, int argsCount)
    {
        MemberName = name;
        ArgsCount = argsCount;
        _signatureLabel.Text = signature;
        if (ArgsCount > 0)
        {
            _argsPanel = _argsPanelScene.Instantiate<ArgsPanel>();
            _argsPanel.Visible = false;
            AddChild(_argsPanel);
        }
    }

    private void OnSceneInstantiated()
    {
        _signatureLabel = GetNode<Label>("%MethodSignature");
        _invokeButton = GetNode<Button>("%InvokeButton");

       
        _invokeButton.Pressed += OnInvoke;
    }

    private void OnInvoke()
    {
        if (ArgsCount > 0)
        {
            _argsPanel.Visible = true;
        }
        else
        {
            Invoke?.Invoke(null);
        }
    }

}