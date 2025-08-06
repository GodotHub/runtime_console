using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuntimeConsole;

/// <summary>
/// 显示对象成员的面板
/// </summary>
public partial class ObjectMemberPanel : TabContainer
{
    public event RequestCreateNewPanelEventHandler CreateNewPanelRequested;
    private ScrollContainer _property;
    private ScrollContainer _field;
    private ScrollContainer _method;
    private ScrollContainer _signal;
    private ScrollContainer _constant;
    private ScrollContainer _element;
    private VBoxContainer _propertyBox;
    private VBoxContainer _fieldBox;
    private VBoxContainer _methodBox;
    private VBoxContainer _signalBox;
    private VBoxContainer _constantBox;
    private VBoxContainer _elementBox;
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
        else if (what == NotificationPredelete)
        {
            // 释放不在树中的选项卡
            Node[] children = [_property, _field, _method, _signal, _constant, _element];
            foreach (Node child in children)
            {
                child.QueueFree();
            }
        }
    }


    private void OnSceneInstantiated()
    {
        _property = GetNode<ScrollContainer>("%Property");
        _field = GetNode<ScrollContainer>("%Field");
        _method = GetNode<ScrollContainer>("%Method");
        _signal = GetNode<ScrollContainer>("%Signal");
        _constant = GetNode<ScrollContainer>("%Constant");
        _element = GetNode<ScrollContainer>("%Element");

        _propertyBox = _property.GetChild<VBoxContainer>(0);
        _fieldBox = _field.GetChild<VBoxContainer>(0);
        _methodBox = _method.GetChild<VBoxContainer>(0);
        _signalBox = _signal.GetChild<VBoxContainer>(0);
        _constantBox = _constant.GetChild<VBoxContainer>(0);
        _elementBox = _element.GetChild<VBoxContainer>(0);
    }

    public void SetParent(string parentProperty)
    {
        Name = parentProperty;
    }

    public void SetObject(object obj, object[] context = null, params IObjectMemberProvider[] providers)
    {
        // 如果是集合，则显示元素面板
        ShowElement(typeof(IEnumerable).IsAssignableFrom(obj.GetType()));

        if (obj is IEnumerable elements)
        {
            var provider = new ElementProvider();
            foreach (var element in provider.Populate(obj, context)) // 传递上下文，处理来自GDScript脚本的枚举数组/字典，为其提供枚举键值
            {
                var control = element.AsControl();

                if (element is IExpendObjectRequester expendObjectRequester)
                {
                    expendObjectRequester.CreateNewPanelRequested += ChildRequestCreateNewPanel;
                }

                AddElement((PropertyEditorBase)control);
            }
        }

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
                            requester.CreateNewPanelRequested += ChildRequestCreateNewPanel;
                        }

                        var propertyEditor = (PropertyEditorBase)control;

                        AddProperty(propertyEditor);
                        break;

                    case MemberEditorType.Field:
                        if (editor is IExpendObjectRequester fieldRequester)
                        {
                            // 转发事件
                            fieldRequester.CreateNewPanelRequested += ChildRequestCreateNewPanel;
                        }

                        var fieldEditor = (PropertyEditorBase)control;

                        AddField(fieldEditor);
                        break;
                }
            }
        }
    }

    private void ChildRequestCreateNewPanel(PropertyEditorBase sender, object obj, object[] context)
    {
        CreateNewPanelRequested?.Invoke(sender, obj, context);
    }

    private void ShowMemberPanel(ScrollContainer memberPanel, bool show)
    {
        if (show)
        {
            if (memberPanel.GetParent() == null)
            {
                AddChild(memberPanel);
            }
        }
        else
        {
            if (memberPanel.GetParent() != null)
            {
                RemoveChild(memberPanel);
            }
        }
    }

    /// <summary>
    /// 显示或隐藏元素面板（对于集合类型）
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowElement(bool show)
    {
        ShowMemberPanel(_element, show);
    }

    /// <summary>
    /// 显示或隐藏属性面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowProperty(bool show)
    {
        ShowMemberPanel(_property, show);
    }


    /// <summary>
    /// 显示或隐藏字段面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowField(bool show)
    {
        ShowMemberPanel(_field, show);
    }


    /// <summary>
    /// 显示或隐藏方法面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowMethod(bool show)
    {
        ShowMemberPanel(_method, show);
    }


    /// <summary>
    /// 显示或隐藏信号面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowSignal(bool show)
    {
        ShowMemberPanel(_signal, show);
    }


    /// <summary>
    /// 显示或隐藏常量面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowConstant(bool show)
    {
        ShowMemberPanel(_constant, show);
    }


    /// <summary>
    /// 向属性面板添加一个属性
    /// </summary>
    /// <param name="editor">属性编辑器</param>
    public void AddProperty(PropertyEditorBase editor)
    {
        _propertyBox.AddChild(editor);
    }


    /// <summary>
    /// 向元素面板添加一个集合元素编辑器
    /// </summary>
    /// <param name="editor">复用属性编辑器充当元素编辑器</param>
    public void AddElement(PropertyEditorBase editor)
    {
        _elementBox.AddChild(editor);
    }

    public void AddField(PropertyEditorBase editor)
    {
        _fieldBox.AddChild(editor);
    }

    public IEnumerable<PropertyEditorBase> GetProperties()
    {
        foreach (var child in _propertyBox.GetChildren())
        {
            yield return (PropertyEditorBase)child;
        }
    }

    public IEnumerable<PropertyEditorBase> GetFields()
    {
        foreach (var child in _fieldBox.GetChildren())
        {
            yield return (PropertyEditorBase)child;
        }
    }
}
