using Godot;
using System;
namespace RuntimeConsole;

public partial class ColorPropertyEditor : PropertyEditorBase
{
    private Color _value;
    private ColorPickerButton _colorPicker;
    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();
        _colorPicker = GetNode<ColorPickerButton>("%ColorPickerButton");
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
        _colorPicker.Disabled = !editable;
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            if (_colorPicker.Color != _value)
            {
                _value = _colorPicker.Color;
                NotificationValueChanged();            
            }
        }
    }

    protected override void SetValue(object value)
    {
        if (value is Color color)
        {
            _colorPicker.Color = color;
            _value = color;
        }
    }

}
