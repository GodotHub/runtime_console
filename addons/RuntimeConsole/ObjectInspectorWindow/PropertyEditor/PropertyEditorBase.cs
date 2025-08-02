using Godot;
using System;
namespace RuntimeConsole;

/// <summary>
/// 属性编辑器基类
/// </summary>
public abstract partial class PropertyEditorBase : PanelContainer
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Property { get; private set; }

    /// <summary>
    /// 属性类型
    /// </summary>
    public Type PropertyType { get; protected set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool Editable { get; protected set; }

    private Label _nameLabel;
    private Button _submitButton;

    /// <summary>
    /// 获取属性值
    /// </summary>
    public abstract object GetValue();

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="value">新的属性值</param>
    public abstract void SetValue(object value);

    /// <summary>
    /// 设置属性是否可编辑
    /// </summary>
    /// <param name="editable">是否可编辑</param>
    public abstract void SetEditable(bool editable);

    /// <summary>
    /// 按下提交按钮后触发, 子类需要实现此方法
    /// </summary>
    protected abstract void OnSubmission();

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("%PropertyName");
        _submitButton = GetNode<Button>("%SubmitButton");

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        _submitButton.Pressed += OnSubmission;
    }

    /// <summary>
    /// 设置属性
    /// </summary>
    /// <param name="property">属性</param>
    public void SetProperty(string name, object value)
    {
        _nameLabel.Text = name;
        Property = name;
        PropertyType = value.GetType();
        SetValue(value);
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
