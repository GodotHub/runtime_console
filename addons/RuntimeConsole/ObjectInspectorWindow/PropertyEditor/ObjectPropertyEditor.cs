using Godot;
using System;

namespace RuntimeConsole;


public partial class ObjectPropertyEditor : PropertyEditorBase, IExpendObjectRequester
{
    public event Action<object, string> RequestCreateNewPanel;
    private object _value;
    private Label _toStringLabel;

    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();
        _toStringLabel = GetNode<Label>("%ObjectToString");
    }


    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
    }

    protected override void SetValue(object value)
    {
        _value = value;
        _toStringLabel.Text = value == null ? "null" : value.ToString();        
    }

    protected override void OnSubmission()
    {
        if (_value != null)
        {
            RequestCreateNewPanel?.Invoke(_value, MemberName);
        }
    }

}
