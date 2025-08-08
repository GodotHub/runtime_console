using Godot;
using System;

namespace RuntimeConsole;

public partial class MethodEditor : InvocableMemberEditor
{
    public override MemberEditorType MemberType => MemberEditorType.Method;
    public int GenericArgsCount { get; private set; }
    public bool PinReturnValue { get; private set; }
    private CheckButton _pinButton;

    public override void SetMemberInfo(string name, string signature, Type[] args, int genericArgsCount = 0)
    {
        GenericArgsCount = genericArgsCount;
        
        base.SetMemberInfo(name, signature, args, genericArgsCount);
    }

    protected override void OnSceneInstantiated()
    {
        _signatureLabel = GetNode<Label>("%MethodSignature");
        _invokeButton = GetNode<Button>("%InvokeButton");
        _pinButton = GetNode<CheckButton>("%PinButton");

        _pinButton.Toggled += enabled => PinReturnValue = enabled;
        _invokeButton.Pressed += OnInvokeButtonPressed;
    }

    protected override void OnInvokeButtonPressed()
    {
        if (ArgsCount > 0 || GenericArgsCount > 0)
        {
            _argsPanel.Visible = true;
        }
        else
        {
            InvokeMember(null);
        }
    }

}