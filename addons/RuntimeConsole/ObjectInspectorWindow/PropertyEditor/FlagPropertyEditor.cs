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

        foreach (Node child in _flagsContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (type.IsEnum)
        {
            _enumValues = Enum.GetValues(type);
            _enumNames = Enum.GetNames(type);
            _flagValue = Convert.ToInt64(value);
        }
        else if (value is ValueTuple<Dictionary<string, int>, Variant> gdsFlag) // 处理GDScript的标志
        {
            var (flagDict, variant) = gdsFlag;
            _enumValues = flagDict.Values.ToArray();
            _enumNames = flagDict.Keys.ToArray();
            _flagValue = variant.AsInt64();
        }

        _tempFlagValue = _flagValue;

        for (int i = 0; i < _enumValues.Length; i++)
        {
            var enumValue = Convert.ToInt64(_enumValues.GetValue(i));
            var enumName = _enumNames[i];

            var checkBox = new CheckBox();
            checkBox.Text = enumName;
            checkBox.ButtonPressed = (_flagValue & enumValue) == enumValue;
            // 使用自定义数据存储索引，避免闭包问题
            int index = i;
            checkBox.Toggled += (t) => OnFlagToggled(t, enumValue, index);

            _flagsContainer.AddChild(checkBox);
        }
    }

    private void OnFlagToggled(bool toggled, long flagValue, int index)
    {
        if (toggled)
        {
            _tempFlagValue |= flagValue;
        }
        else
        {
            _tempFlagValue &= ~flagValue;
        }
        
        // 更新其他可能受影响的复选框状态（处理组合值）
        UpdateRelatedCheckBoxes();
    }

    // 处理组合值的显示更新
    private void UpdateRelatedCheckBoxes()
    {
        for (int i = 0; i < _enumValues.Length; i++)
        {
            var enumValue = Convert.ToInt64(_enumValues.GetValue(i));
            var checkBox = _flagsContainer.GetChild<CheckBox>(i);
            
            // 更新每个复选框的状态以反映当前的 _tempFlagValue
            checkBox.SetPressedNoSignal((_tempFlagValue & enumValue) == enumValue);
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
