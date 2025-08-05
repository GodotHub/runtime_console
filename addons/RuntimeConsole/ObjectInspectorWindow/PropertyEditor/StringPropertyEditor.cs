using Godot;
using System;

namespace RuntimeConsole;

public partial class StringPropertyEditor : PropertyEditorBase
{
    private TextEdit _textEdit;
    private object _value;

    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();

        _textEdit ??= GetNode<TextEdit>("%ValueEditor");     
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {            
            var oldValue = _value;
            SetValue(_textEdit.Text);

            if (!oldValue.Equals(_value))
                NotificationValueChanged();
        }
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
        _textEdit.Editable = editable;
        _editButton.Disabled = !editable;
    }

    protected override void SetValue(object value)
    {
        if (value is not (string or StringName or NodePath))
            return;

        _textEdit.Text = value.ToString();

        if (PropertyType == typeof(StringName))
        {
            _value = new StringName(value.ToString());
        }
        else if (PropertyType == typeof(NodePath))
        {
            _value = new NodePath(value.ToString());
        }
        else
        {
            _value = value.ToString();
        }
    }
}
