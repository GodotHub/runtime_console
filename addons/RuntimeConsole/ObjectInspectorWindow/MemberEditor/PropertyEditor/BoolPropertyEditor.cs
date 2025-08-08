using Godot;
using System;
namespace RuntimeConsole;

public partial class BoolPropertyEditor : PropertyEditorBase
{
    private CheckBox _checkBox;
    private bool _value;

    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();
        _checkBox ??= GetNode<CheckBox>("%ValueEditor");
        _checkBox.Pressed += () => _checkBox.Text = _checkBox.ButtonPressed.ToString();
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

    protected override void SetValue(object value)
    {
        if (value is not bool boolValue)
            return;
        
        _checkBox.ButtonPressed = boolValue;
        _checkBox.Text = boolValue.ToString();
        _value = boolValue;
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            if (_value != _checkBox.ButtonPressed)
            {
                _value = _checkBox.ButtonPressed;
                NotificationValueChanged();
            }
        }
    }

}
