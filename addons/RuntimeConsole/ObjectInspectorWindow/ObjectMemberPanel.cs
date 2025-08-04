using Godot;
using System;
using System.Linq;

namespace RuntimeConsole;

/// <summary>
/// 显示对象成员的面板
/// </summary>
public partial class ObjectMemberPanel : TabContainer
{
    public event Action<object, string> CreateNewPanelRequested;
    public object ParentObject { get; private set; }
    public string ParentPropertyName { get; private set; }
    public object CurrentObject;    
    private ScrollContainer _property;
    private ScrollContainer _field;
    private ScrollContainer _method;
    private ScrollContainer _signal;
    private ScrollContainer _constant;
    private VBoxContainer _propertyBox;
    private VBoxContainer _fieldBox;
    private VBoxContainer _methodBox;
    private VBoxContainer _signalBox;
    private VBoxContainer _constantBox;
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }
 
    private void OnSceneInstantiated()
    {
        _property = GetNode<ScrollContainer>("%Property");
        _field = GetNode<ScrollContainer>("%Field");
        _method = GetNode<ScrollContainer>("%Method");
        _signal = GetNode<ScrollContainer>("%Signal");
        _constant = GetNode<ScrollContainer>("%Constant");

        _propertyBox = _property.GetChild<VBoxContainer>(0);
        _fieldBox = _field.GetChild<VBoxContainer>(0);
        _methodBox = _method.GetChild<VBoxContainer>(0);
        _signalBox = _signal.GetChild<VBoxContainer>(0);
        _constantBox = _constant.GetChild<VBoxContainer>(0);
    }

    public void SetParent(object parent, string propertyName)
    {
        ParentObject = parent;
        ParentPropertyName = propertyName;
    }

    public void SetObject(object obj, params IObjectMemberProvider[] providers)
    {
        CurrentObject = obj;

        foreach (var provider in providers)
        {
            foreach (var editor in provider.Populate(obj))
            {
                var control = editor.AsControl();
                switch (editor.MemberType)
                {
                    case MemberEditorType.Property:
                        if (editor is IExpendObjectRequester requester)
                        {
                            // 转发事件
                            requester.RequestCreateNewPanel += ChildRequestCreateNewPanel;
                        }

                        var propertyEditor = (PropertyEditorBase)control;

                        AddProperty(propertyEditor);
                        break;
                }
            }
        }
    }

    private void ChildRequestCreateNewPanel(object obj, string memberName)
    {
        CreateNewPanelRequested?.Invoke(obj, memberName);
    }

    /// <summary>
    /// 显示或隐藏属性面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowProperty(bool show)
        => _property.Visible = show;

    /// <summary>
    /// 显示或隐藏字段面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowField(bool show)
        => _field.Visible = show;

    /// <summary>
    /// 显示或隐藏方法面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowMethod(bool show)
        => _method.Visible = show;

    /// <summary>
    /// 显示或隐藏信号面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowSignal(bool show)
        => _signal.Visible = show;

    /// <summary>
    /// 显示或隐藏常量面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowConstant(bool show)
        => _constant.Visible = show;

    /// <summary>
    /// 向属性面板添加一个属性
    /// </summary>
    /// <param name="editor">属性编辑器</param>
    public void AddProperty(PropertyEditorBase editor)
        => _propertyBox.AddChild(editor);

    /// <summary>
    /// 根据属性名称获取属性编辑器
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <returns>如果成功获取则返回对应编辑器，否则返回null</returns>
    public PropertyEditorBase GetProperty(string name)
    {
        foreach (var child in _propertyBox.GetChildren().Cast<PropertyEditorBase>())
        {
            if (child.MemberName == name)
            {
                return child;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取指定索引处的属性编辑器
    /// </summary>
    /// <param name="index">索引</param>
    /// <returns>如果成功获取则返回对应编辑器，否则返回null</returns>
    public PropertyEditorBase GetProperty(int index)
    {
        var prop = _propertyBox.GetChildren().Cast<PropertyEditorBase>();
        if (index < prop.Count())
        {
            return prop.ElementAt(index);
        }
        return null;
    }
}
