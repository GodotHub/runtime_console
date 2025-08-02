using Godot;
using System;

namespace RuntimeConsole;

public partial class StringPropertyEditor : PropertyEditorBase
{
    private TextEdit _textEdit;
    private string _value;

    public override void _Ready()
    {
        base._Ready();

        _textEdit = GetNode<TextEdit>("%TextEdit");     
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            SetValue(_textEdit.Text);
        }
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        _textEdit.Editable = editable;
    }

    public override void SetValue(object value)
    {
        _textEdit.Text = value.ToString();
        _value = value.ToString();
    }
}
