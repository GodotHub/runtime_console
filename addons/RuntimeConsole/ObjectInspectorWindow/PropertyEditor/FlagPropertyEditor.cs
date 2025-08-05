using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

public partial class FlagPropertyEditor : PropertyEditorBase
{
    private VBoxContainer _flagsContainer;
    private long _flagValue;
    private long _tempFlagValue;

    private Array _enumValues;
    private string[] _enumNames;

    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();
        _flagsContainer = GetNode<VBoxContainer>("%FlagsContainer");
    }

    protected override void SetProperty(string name, Type type, object value)
    {
        _nameLabel.Text = name;
        _typeLabel.Text = type.ToString();
        MemberName = name;
        PropertyType = type;

        _flagValue = Convert.ToInt64(value);
        _tempFlagValue = _flagValue;

        foreach (Node child in _flagsContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (type.IsEnum)
        {
            _enumValues = Enum.GetValues(type);
            _enumNames = Enum.GetNames(type);

            for (int i = 0; i < _enumValues.Length; i++)
            {
                var enumValue = Convert.ToInt64(_enumValues.GetValue(i));
                var enumName = _enumNames[i];

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

    protected override void SetValue(object value)
    {
        // if (!Enum.IsDefined(PropertyType, value))
        //     return;

        _flagValue = Convert.ToInt64(value);
        // _tempFlagValue = _flagValue;
        
        // 更新UI中的CheckBox状态
        // UpdateCheckBoxes();
    }

    // private void UpdateCheckBoxes()
    // {
    //     if (_flagsContainer == null || PropertyType == null)
    //         return;
            
    //     var children = _flagsContainer.GetChildren();
    //     if (children.Count() == 0)
    //         return;
                    
    //     var checkBoxes = _flagsContainer.GetChildren().Cast<CheckBox>().ToList();
        
    //     for (int i = 0; i < _enumValues.Length && i < checkBoxes.Count; i++)
    //     {
    //         var enumValue = Convert.ToInt64(_enumValues.GetValue(i));
    //         var checkBox = checkBoxes[i];
    //         checkBox.ButtonPressed = (_flagValue & enumValue) != 0;
    //     }
    // }
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
