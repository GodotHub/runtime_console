using Godot;
using System;
using System.Collections;

namespace RuntimeConsole;

public partial class CollectionPropertyEditor : PropertyEditorBase, IExpendObjectRequester
{
    public event RequestCreateNewPanelEventHandler CreateNewPanelRequested;
    private IEnumerable _value;
    private bool _mutable;    

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
        _editButton.Disabled = !editable;
    }

    protected override void OnSubmission()
    {
        if (_value != null)
        {
            CreateNewPanelRequested?.Invoke(this, _value);
        }
    }

    protected override void SetValue(object value)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(value.GetType()))
        {
            return;
        }

        _value = (IEnumerable)value;
    }
    
    public void OnPanelCreated(ObjectMemberPanel panel)
    {

    }
}
