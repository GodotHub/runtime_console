using Godot;
using System;

namespace RuntimeConsole;


public partial class ObjectPropertyEditor : PropertyEditorBase, IExpendObjectRequester
{
    public event RequestCreateNewPanelEventHandler CreateNewPanelRequested;
    private object _value;
    private Label _toStringLabel;
    private object[] _context;

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
            CreateNewPanelRequested?.Invoke(this, _value, _context);
        }
    }

    // 不需要使用新创建的面板进行操作，这里直接空实现
    public void OnPanelCreated(ObjectMemberPanel panel)
    {

    }

    public void SetContext(object[] context)
    {
        _context = context;
    }

}
