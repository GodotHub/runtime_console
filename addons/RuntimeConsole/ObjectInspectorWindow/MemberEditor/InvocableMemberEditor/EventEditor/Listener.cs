using Godot;
using System;

namespace RuntimeConsole;

public partial class Listener : PanelContainer
{
    public event Action<string> Removed;
    private Label _nameLabel;
    private Button _removeButton;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }

    }

    public void SetRemovable(bool removable)
    {
        _removeButton.Visible = removable;
    }

    private void OnSceneInstantiated()
    {
        _nameLabel = GetNode<Label>("%NameLabel");
        _removeButton = GetNode<Button>("%RemoveButton");

        _removeButton.Pressed += OnRemoveButtonPressed;
    }

    public void SetListener(string name)
    {
        _nameLabel.Text = name;            
    }

    private void OnRemoveButtonPressed()
    {
        Removed?.Invoke(_nameLabel.Text);
        QueueFree();
    }
}
