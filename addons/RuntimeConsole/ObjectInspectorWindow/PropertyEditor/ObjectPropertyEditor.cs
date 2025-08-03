using Godot;
using System;

namespace RuntimeConsole;

public partial class ObjectPropertyEditor : PropertyEditorBase
{
    private object _value;
    private Label _toStringLabel;

    public override void _Ready()
    {
        base._Ready();
        _toStringLabel = GetNode<Label>("%ObjectToString");
   }


    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        _editButton.Disabled = !editable;
    }

    public override void SetValue(object value)
    {
        _value = value;
        _toStringLabel.Text = value == null ? "null" :value.ToString();
        NotificationValueChanged();
    }

    protected override void OnSubmission()
    {
        
    }

}
