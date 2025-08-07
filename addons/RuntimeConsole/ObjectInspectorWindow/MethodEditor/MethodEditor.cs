using Godot;
using System;

namespace RuntimeConsole;

public partial class MethodEditor : PanelContainer, IMemberEditor
{
    public event Action<object[]> Invoke;
    public string MemberName { get; private set; }

    public MemberEditorType MemberType => MemberEditorType.Method;
    public int ArgsCount { get; private set; }
    public bool PinReturnValue { get; private set; }

    public Control AsControl() => this;
    private Label _signatureLabel;
    private Button _invokeButton;
    private ArgsPanel _argsPanel = null;
    private CheckButton _pinButton;
    private static readonly PackedScene _argsPanelScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MethodEditor/ArgsPanel.tscn");

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }

    public void SetMethodInfo(string name, string signature, Type[] args)
    {
        MemberName = name;
        ArgsCount = args.Length;
        _signatureLabel.Text = signature;
        if (ArgsCount > 0)
        {
            _argsPanel = _argsPanelScene.Instantiate<ArgsPanel>();
            _argsPanel.Visible = false;
            _argsPanel.MethodInvoked += args => Invoke?.Invoke(args);
            _argsPanel.SetArgs(args);
            AddChild(_argsPanel);
        }
    }

    private void OnSceneInstantiated()
    {
        _signatureLabel = GetNode<Label>("%MethodSignature");
        _invokeButton = GetNode<Button>("%InvokeButton");
        _pinButton = GetNode<CheckButton>("%PinButton");

        _pinButton.Toggled += enabled => PinReturnValue = enabled;
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