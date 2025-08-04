using Godot;
using System;

namespace RuntimeConsole;

public partial class NumberPropertyEditor : PropertyEditorBase
{
    private SpinBox _spinBox;
    private object _value;

    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();
        _spinBox = GetNode<SpinBox>("%ValueEditor");
        var lineEdit = _spinBox.GetLineEdit();
        lineEdit.ContextMenuEnabled = false;        
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            var oldValue = _value;
            OnSpinBoxValueChanged(_spinBox.Value);

            if (!_value.Equals(oldValue))
                NotificationValueChanged();
        }
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        _editButton.Disabled = !editable;
        _spinBox.Editable = editable;
    }

    protected override void SetValue(object value)
    {
        _value = value;
        
        switch (value)
        {
            case sbyte sbyteValue:
                SetSpinBoxContent(sbyteValue, sbyte.MinValue, sbyte.MaxValue, 1);
                break;
            case byte byteValue:
                SetSpinBoxContent(byteValue, byte.MinValue, byte.MaxValue, 1);
                break;
            case short shortValue:
                SetSpinBoxContent(shortValue, short.MinValue, short.MaxValue, 1);
                break;
            case ushort ushortValue:
                SetSpinBoxContent(ushortValue, ushort.MinValue, ushort.MaxValue, 1);
                break;
            case int intValue:
                SetSpinBoxContent(intValue, int.MinValue, int.MaxValue, 1);
                break;
            case uint uintValue:
                SetSpinBoxContent(uintValue, uint.MinValue, uint.MaxValue, 1);
                break;
            case long longValue:
                SetSpinBoxContent(longValue, long.MinValue, long.MaxValue, 1);
                break;
            case ulong ulongValue:
                SetSpinBoxContent(ulongValue, ulong.MinValue, ulong.MaxValue, 1);
                break;
            case float floatValue:                
                // 这个浮点数最小值不能改，14位有效数字，改了会忽略UI控件的值变化
                SetSpinBoxContent(floatValue, -3.4028235E+14, float.MaxValue, GetStepForFloat(floatValue));
                break;
            case double doubleValue:
                SetSpinBoxContent(doubleValue, -3.4028235E+14, double.MaxValue, GetStepForFloat(doubleValue));
                break;
            case decimal decimalValue:
                SetSpinBoxContent((double)decimalValue, -3.4028235E+14, double.MaxValue, GetStepForFloat((double)decimalValue));
                break;
            default:
                if (value != null)
                {
                    // 尝试转换为double
                    if (double.TryParse(value.ToString(), out double parsedValue))
                    {
                        SetSpinBoxContent(parsedValue,  -3.4028235E+14, double.MaxValue, GetStepForFloat(parsedValue));
                        return;
                    }
                }
                // 默认设置
                SetSpinBoxContent(0, -3.4028235E+14, double.MaxValue, 1);
                break;
        }
    }

    private void SetSpinBoxContent(double value, double minValue, double maxValue, double step)
    {        
        _spinBox.MinValue = minValue;
        _spinBox.MaxValue = maxValue;        
        _spinBox.Step = step;
        _spinBox.Value = value;
    }

    private double GetStepForFloat(double value)
    {
        // 对于浮点数，根据数值大小动态调整步长
        if (Math.Abs(value) < 1e-3)
            return 1e-6;
        else if (Math.Abs(value) < 1e-1)
            return 1e-3;
        else if (Math.Abs(value) < 10)
            return 0.1;
        else
            return 1;
    }

    private void OnSpinBoxValueChanged(double value)
    {
        // 根据原始值的类型进行适当的类型转换
        if (_value != null)
        {
            try
            {
                Type valueType = _value.GetType();
                _value = Convert.ChangeType(value, valueType);
            }
            catch
            {
                // 如果转换失败，保留double值
                _value = value;
            }
        }
        else
        {
            _value = value;
        }
    }
}