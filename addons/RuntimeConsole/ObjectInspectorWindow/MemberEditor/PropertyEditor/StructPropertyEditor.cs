using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuntimeConsole;

public partial class StructPropertyEditor : PropertyGroupEditor, IExpendObjectRequester
{
    public event RequestCreateNewPanelEventHandler CreateNewPanelRequested;
    
    private object _value;
    private object[] _context;
    private List<PropertyEditorBase> _childProperties = [];
    private Label _toStringLabel;
    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();
        _toStringLabel = GetNode<Label>("%StructToString");
    }
    public override object GetValue()
    {        
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
        _childProperties.ForEach(x => x.SetEditable(editable));
    }

    protected override void OnSubmission()
    {
        if (_value != null)
        {
            CreateNewPanelRequested?.Invoke(this, _value, _context);
        }
    }

    protected override void SetValue(object value)
    {
        _value = value;
        _toStringLabel.Text = value == null ? "null" : value.ToString();        
    }

    public void OnPanelCreated(ObjectMemberPanel panel)
    {
        _childProperties.Clear();
        panel.ShowElement(false);

        foreach (var prop in panel.GetProperties())
        {
            _childProperties.Add(prop);
            prop.ValueChanged += (value) => OnChildValueChanged(prop, value);
        }

        foreach (var field in panel.GetFields())
        {
            _childProperties.Add(field);
            field.ValueChanged += (value) => OnChildValueChanged(field, value);
        }
    }

    protected override IEnumerable<PropertyEditorBase> GetChildProperties()
    {
        return _childProperties;
    }

    protected override void OnChildValueChanged(PropertyEditorBase _, object value)
    {        
        var newInstance = Activator.CreateInstance(PropertyType);
        foreach (var child in _childProperties)
        {
            if (child.MemberType == MemberEditorType.Property)
            {
                try
                {
                    newInstance.GetType().GetProperty(child.MemberName).SetValue(newInstance, child.GetValue());
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Property '{child.MemberName}' has been skipped because it failed to set the value : {ex.Message}");
                    continue;
                }
            }
            else if (child.MemberType == MemberEditorType.Field)
            {
                try
                {
                    newInstance.GetType().GetField(child.MemberName).SetValue(newInstance, child.GetValue());
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Field '{child.MemberName}' has been skipped because it failed to set the value : {ex.Message}");
                    continue;
                }                
            }     
        }

        _value = newInstance;
        NotificationValueChanged();
    }

    public void SetContext(object[] context)
    {
        _context = context;
    }
}
