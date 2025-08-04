using Godot;
using System;
namespace RuntimeConsole;

public delegate void PropertyChanged(object value);

/// <summary>
/// 属性编辑器基类
/// </summary>
public abstract partial class PropertyEditorBase : PanelContainer, IMemberEditor
{

    public event PropertyChanged ValueChanged;

    public MemberEditorType MemberType => MemberEditorType.Property;

    /// <summary>
    /// 属性名称
    /// </summary>
    public string MemberName { get; protected set; }

    /// <summary>
    /// 属性类型
    /// </summary>
    public Type PropertyType { get; protected set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool Editable { get; protected set; } = true;

    protected Label _nameLabel;
    protected Label _typeLabel;
    protected Button _editButton;

    /// <summary>
    /// 获取属性值
    /// </summary>
    public abstract object GetValue();

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="value">新的属性值</param>
    protected abstract void SetValue(object value);

    /// <summary>
    /// 设置属性是否可编辑
    /// </summary>
    /// <param name="editable">是否可编辑</param>
    public abstract void SetEditable(bool editable);

    /// <summary>
    /// 按下提交按钮后触发, 子类需要实现此方法
    /// </summary>
    protected abstract void OnSubmission();

    public Control AsControl() => this;

    public override void _Notification(int what)
    {
        // 场景实例化后
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }

    protected virtual void OnSceneInstantiated()
    {
        _nameLabel = GetNode<Label>("%PropertyName");
        _editButton = GetNode<Button>("%EditButton");
        _typeLabel = GetNode<Label>("%PropertyType");

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        _editButton.Pressed += OnSubmission;
    }

    /// <summary>
    /// 设置此编辑器关联的属性，控件的初始化逻辑
    /// </summary>
    /// <param name="name">属性名</param>
    /// <param name="type">属性类型</param>
    /// <param name="value">属性值</param>
    public virtual void SetProperty(string name, Type type, object value)
    {
        _nameLabel.Text = name;
        _typeLabel.Text = type.FullName;
        MemberName = name;
        PropertyType = type;
        SetValue(value);
    }

    protected void NotificationValueChanged()
    {
        ValueChanged?.Invoke(GetValue());
    }

    private void OnMouseEntered()
    {
        Modulate = new Color(1, 1, 1, 0.5f);
    }

    private void OnMouseExited()
    {
        Modulate = new Color(1, 1, 1, 1);
    }
}
