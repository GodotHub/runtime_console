using Godot;
using System;

namespace RuntimeConsole;

public abstract partial class InvocableMemberEditor : PanelContainer, IMemberEditor
{
    public event Action<object[]> Invoke;

    public string MemberName { get; protected set; }
    public abstract MemberEditorType MemberType { get; }
    public int ArgsCount { get; protected set; }

    public Control AsControl() => this;
    protected static readonly PackedScene _argsPanelScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/InvocableMemberEditor/ArgPanel/ArgsPanel.tscn");

    protected Label _signatureLabel;
    protected Button _invokeButton;
    protected ArgsPanel _argsPanel;

    /// <summary>
    /// 设置成员信息
    /// </summary>
    /// <param name="name">方法/事件的名字</param>
    /// <param name="signature">方法/事件的签名</param>
    /// <param name="args">方法/事件所需的参数类型</param>
    /// <param name="genericArgsCount">指示该方法有多少个泛型参数</param>
    public virtual void SetMemberInfo(string name, string signature, Type[] args, int genericArgsCount = 0)
    {
        MemberName = name;
        ArgsCount = args.Length;
        _signatureLabel.Text = signature;

        if (ArgsCount > 0 || genericArgsCount > 0)
        {
            CreateArgsPanel(genericArgsCount, args);
        }
    }

    protected void CreateArgsPanel(int genericArgsCount, Type[] args)
    {
        _argsPanel = _argsPanelScene.Instantiate<ArgsPanel>();
        _argsPanel.Visible = false;
        _argsPanel.Invoked += InvokeMember;
        _argsPanel.SetArgs(_signatureLabel.Text, genericArgsCount, args);
        AddChild(_argsPanel);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }
    }

    protected abstract void OnSceneInstantiated();

    protected abstract void OnInvokeButtonPressed();

    protected void InvokeMember(object[] args)
    {
        Invoke?.Invoke(args);
    }

    private void OnMouseEntered()
    {
        Modulate = new Color(1, 1, 1, 0.5f);
    }

    private void OnMouseExited()
    {
        Modulate = new Color(1, 1, 1, 1);
    }
}