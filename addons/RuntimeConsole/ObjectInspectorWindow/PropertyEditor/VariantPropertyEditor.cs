using System;
using Godot;

namespace RuntimeConsole;

public partial class VariantPropertyEditor : PropertyEditorBase, IExpendObjectRequester
{
    private Variant _value;

    public event RequestCreateNewPanelEventHandler CreateNewPanelRequested;

    // 这里重写父类的初始化逻辑，重写成空实现，因为该编辑器是根据构造时传入的Variant自动分派其他编辑器，不需要场景初始化
    protected override void OnSceneInstantiated()
    {
        return;
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;

        if (GetChildCount() > 0)
        {
            (GetChild(0) as PropertyEditorBase)?.SetEditable(editable);
        }
    }

    // 这里重写为空实现，因为提交修改由该编辑器创建的子编辑器处理
    protected override void OnSubmission()
    {
        return;
    }

    protected override void SetProperty(string name, Type type, object value)
    {
        MemberName = name;
        PropertyType = type;
        SetValue(value);
    }

    protected override void SetValue(object value)
    {
        if (value is not Variant variant)
            return;

        _value = variant;

        // type为空，即Variant没有赋值
        var type = (variant.Obj?.GetType()) ?? typeof(object);

        var editor = PropertyEditorFactory.Create(type);
        
        editor.SetEditable(Editable);
        editor.SetMemberInfo(MemberName, typeof(Variant), variant.Obj, MemberType);
        editor.Name = MemberName;
        editor.ValueChanged += OnValueChanged;

        // 转发事件
        if (editor is IExpendObjectRequester requester)
            requester.CreateNewPanelRequested += OnCreateNewPanelRequested;
            
        AddChild(editor);
    }

    private void OnValueChanged(object value)
    {
        if (!value.Equals(_value.Obj))
        {            
            _value = VariantUtility.Create(value);
            NotificationValueChanged();
        }
    }

    private void OnCreateNewPanelRequested(PropertyEditorBase editor, object obj)
        => CreateNewPanelRequested?.Invoke(editor, obj);

    public void OnPanelCreated(ObjectMemberPanel panel)
    {

    }
}