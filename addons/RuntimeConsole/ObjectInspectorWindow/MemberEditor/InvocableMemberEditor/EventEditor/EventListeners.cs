using Godot;
using System;
using System.Collections.Generic;

namespace RuntimeConsole;

public partial class EventListeners : PopupPanel
{
    public event Action<string> ListenerRemoved;
    private Label _emptyLabel;
    private Label _signatureLabel;
    private VBoxContainer _listenersBox;
    private static readonly PackedScene _listenerScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/InvocableMemberEditor/EventEditor/Listener.tscn");
    public override void _Ready()
    {
        _emptyLabel = GetNode<Label>("%EmptyLabel");
        _signatureLabel = GetNode<Label>("%SignatureLabel");
        _listenersBox = GetNode<VBoxContainer>("%ListenersContainer");        
    }

    public void Clear()
    {
        foreach (var child in _listenersBox.GetChildren())
        {
            child.QueueFree();
        }
        _emptyLabel.Visible = true;
    }

    public void SetListeners(string signature,  Dictionary<string, (object listener, object eventHandler)> listeners)
    {
        _signatureLabel.Text = signature;

        foreach (var listener in listeners)
        {
            var listenerEntry = _listenerScene.Instantiate<Listener>();
            listenerEntry.SetListener(listener.Key);
            listenerEntry.SetRemovable(listener.Value.listener != null);
            listenerEntry.Removed += OnListenerRemoved;
            _listenersBox.AddChild(listenerEntry);
        }

        _emptyLabel.Visible = listeners.Count == 0;
    }

    private void OnListenerRemoved(string listener)
    {
        ListenerRemoved?.Invoke(listener);
    }
}
