using Godot;
using System;
namespace RuntimeConsole;

public partial class BoolPropertyEditor : PropertyEditorBase
{
    private CheckBox _checkBox;
    private bool _value;

    public override void _Ready()
    {
        base._Ready();
        _checkBox = GetNode<CheckBox>("%ValueEditor");
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        _editButton.Disabled = !editable;
        _checkBox.Disabled = !editable;
    }

    public override void SetValue(object value)
    {
        if (value is not bool boolValue)
            return;
        
        _checkBox.ButtonPressed = boolValue;
        _checkBox.Text = boolValue.ToString();
        _value = boolValue;
        NotificationValueChanged();
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            SetValue(_checkBox.ButtonPressed);
        }
    }

}
