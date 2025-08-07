using Godot;
using System;

namespace RuntimeConsole;

public partial class ClipboardEntry : HBoxContainer
{
    public event Action<int> Removed;
    private object _value;
    private int _index;
    private Label _indexLabel;
    private Label _valueLabel;
    private Button _removeButton;

    public override void _Notification(int what)
    {    
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }


    public void OnSceneInstantiated()
    {
        _indexLabel = GetNode<Label>("%Index");
        _valueLabel = GetNode<Label>("%Value");
        _removeButton = GetNode<Button>("%Remove");

        _removeButton.Pressed += OnButtonPressed;
    }

    public void SetValue(object value, int index)
    {
        _value = value;
        _index = index;        
        _indexLabel.Text = index.ToString();
        _valueLabel.Text = value?.ToString() ?? "null";
    }

    public object GetValue()
    {
        return _value;
    }

    private void OnButtonPressed()
    {
        Removed.Invoke(_index);
    }

}
