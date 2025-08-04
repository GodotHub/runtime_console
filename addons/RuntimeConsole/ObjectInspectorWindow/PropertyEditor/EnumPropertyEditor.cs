using Godot;
using System;
using System.Collections.Generic;

namespace RuntimeConsole;

public partial class EnumPropertyEditor : PropertyEditorBase
{
    [Export] private OptionButton _optionButton;
    private long _value;
    private string[] _enumNames;
    private Array _enumValues;

    public override void _Ready()
    {
        base._Ready();
        _optionButton ??= GetNode<OptionButton>("%ValueEditor");
        // _optionButton.GetPopup().AlwaysOnTop = true;
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

    protected override void SetProperty(string name, Type type, object value)
    {
        _nameLabel.Text = name;
        _typeLabel.Text = type.FullName;
        MemberName = name;
        PropertyType = type;
        if (type.IsEnum)
        {
            _enumValues = Enum.GetValues(type);
            _enumNames = Enum.GetNames(type);
            foreach (var item in _enumNames)
            {
                _optionButton.AddItem(item);
            }
        }        
        SetValue(value);
    }

    protected override void SetValue(object value)
    {               
        if (value is Enum enumValue)
        {
            _value = Convert.ToInt64(enumValue);

            for (int i = 0; i < _enumNames.Length; i++)
            {
                long enumValueAsLong = Convert.ToInt64(_enumValues.GetValue(i));

                if (enumValueAsLong == _value)
                {
                    _optionButton.Select(i);
                    break;
                }

            }
        }

        if (value is int selectedIdx)
        {
            var name = _optionButton.GetItemText(selectedIdx);            

            for (int i = 0; i < _enumNames.Length; i++)
            {
                if (_enumNames[i] == name)
                {
                    _value = Convert.ToInt64(_enumValues.GetValue(i));                    
                    break;
                }
            }
        }
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {

            var selected = _optionButton.GetSelected();
            if (_value != selected)
            {
                SetValue(selected);
                NotificationValueChanged();
            }
        }
    }

}
