using Godot;
using System;
using System.Linq;

namespace RuntimeConsole;

public partial class FlagPropertyEditor : PropertyEditorBase
{
    private VBoxContainer _flagsContainer;
    private long _flagValue;
    private long _tempFlagValue;

    public override void _Ready()
    {
        base._Ready();
        _flagsContainer = GetNode<VBoxContainer>("%FlagsContainer");
    }

    public override void SetProperty(string name, Type type, object value)
    {
        _nameLabel.Text = name;
        _typeLabel.Text = type.FullName;
        Property = name;
        PropertyType = type;

        _flagValue = Convert.ToInt64(value);
        _tempFlagValue = _flagValue;

        foreach (Node child in _flagsContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (type.IsEnum)
        {
            var enumValues = Enum.GetValues(type);
            var enumNames = Enum.GetNames(type);

            for (int i = 0; i < enumValues.Length; i++)
            {
                var enumValue = Convert.ToInt64(enumValues.GetValue(i));
                var enumName = enumNames[i];

                var checkBox = new CheckBox();
                checkBox.Text = enumName;
                checkBox.ButtonPressed = (_flagValue & enumValue) != 0;
                checkBox.Toggled += (t) => OnFlagToggled(t, enumValue);

                _flagsContainer.AddChild(checkBox);
            }
        }
    }

    private void OnFlagToggled(bool toggled, long flagValue)
    {
        if (toggled)
        {
            _tempFlagValue |= flagValue;
        }
        else
        {
            _tempFlagValue &= ~flagValue;
        }         
    }

    public override object GetValue()
    {
        return _flagValue;
    }

    public override void SetEditable(bool editable)
    {
        _editButton.Disabled = !editable;
        _flagsContainer.GetChildren()
            .Cast<CheckBox>()
            .ToList()
            .ForEach(child => child.Disabled = !editable);
    }

    public override void SetValue(object value)
    {
        _flagValue = Convert.ToInt64(value);
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            if (_flagValue != _tempFlagValue)
            {
                SetValue(_tempFlagValue);
                NotificationValueChanged();
            }
        }
    }

}
