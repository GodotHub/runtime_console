using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

public partial class EventEditor : InvocableMemberEditor
{    
    public override MemberEditorType MemberType => MemberEditorType.Event;
    private EventListeners _listenersPanel;
    private Button _viewListenersButton;
    private Dictionary<string, (object listener, object eventHandler)> _listeners = [];
    public event Action<object, object> ListenerRemoved;

    public override void SetMemberInfo(string name, string signature, Type[] args, int genericArgsCount = 0)
    {
        MemberName = name;
        ArgsCount = args.Length;
        _signatureLabel.Text = signature;

        // 事件不能有类型参数，这里忽略它，只判断所需参数数量
        if (ArgsCount > 0)
        {
            CreateArgsPanel(genericArgsCount, args);
        }
    }

    public void SetListeners(Dictionary<string, (object, object)> listeners)
    {
        _listeners = listeners;    
    }

    protected override void OnSceneInstantiated()
    {
        _signatureLabel = GetNode<Label>("%EventSignature");
        _invokeButton = GetNode<Button>("%InvokeButton");
        _listenersPanel = GetNode<EventListeners>("%EventListeners");
        _viewListenersButton = GetNode<Button>("%ListenerButton");

        _listenersPanel.ListenerRemoved += OnListenerRemoved;
        _viewListenersButton.Pressed += OnViewListenersButtonPressed;
        _invokeButton.Pressed += OnInvokeButtonPressed;
    }

    private void OnViewListenersButtonPressed()
    {
        _listenersPanel.Clear();
        _listenersPanel.SetListeners(_signatureLabel.Text, _listeners); 
        _listenersPanel.PopupCentered();
    }

    private void OnListenerRemoved(string key)
    {
        var (listener, eventHandler) = _listeners[key];
        ListenerRemoved?.Invoke(listener, eventHandler);
        _listeners.Remove(key);
    }

    protected override void OnInvokeButtonPressed()
    {
        if (ArgsCount > 0)
        {
            _argsPanel.Visible = true;
        }
        else
        {
            InvokeMember(null);
        }
    }

}
