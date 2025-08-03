using Godot;
using System;
using System.Collections.Generic;

namespace RuntimeConsole;

public partial class EnumPropertyEditor : PropertyEditorBase
{
    private OptionButton _optionButton;

    private long _value;

    public override void _Ready()
    {
        base._Ready();
        _optionButton = GetNode<OptionButton>("%ValueEditor");
        _optionButton.GetPopup().AlwaysOnTop = true;
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        _editButton.Disabled = !editable;
        _optionButton.Disabled = !editable;
    }

    public override void SetProperty(string name, Type type, object value)
    {
        _nameLabel.Text = name;
        _typeLabel.Text = type.FullName;
        Property = name;
        PropertyType = type;     
        if (value is Enum)
        {            
            foreach (var item in Enum.GetNames(PropertyType))
            {
                _optionButton.AddItem(item);
            }
        }        
    }

    public override void SetValue(object value)
    {
        if (value is Enum enumValue)
        {
            _value = Convert.ToInt64(enumValue);
            var names = Enum.GetNames(PropertyType);
            var values = Enum.GetValues(PropertyType);

            for (int i = 0; i < names.Length; i++)
            {
                long enumValueAsLong = Convert.ToInt64(values.GetValue(i));

                if (enumValueAsLong == _value)
                {
                    _optionButton.Select(i);
                }

            }
        }

        if (value is int selectedIdx)
        {
            var name = _optionButton.GetItemText(selectedIdx);
            var names = Enum.GetNames(PropertyType);
            var values = Enum.GetValues(PropertyType);

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    _value = Convert.ToInt64(values.GetValue(i));                    
                    break;
                }
            }
        }
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            SetValue(_optionButton.GetSelected());
            NotificationValueChanged();
        }
    }

}
