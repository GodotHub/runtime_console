using Godot;
using System;
using System.Reflection;

namespace RuntimeConsole;

public partial class ObjectInspectorWindow : Window
{
    private ObjectTreeWindow _objTree;
    private Label _objName;
    private Label _objRID;
    private VBoxContainer _objProperties;
    private object _obj;
    public override void _Ready()
    {
        Size = (Vector2I)GetTree().Root.GetViewport().GetVisibleRect().Size / 2;

        _objTree = Console.GameConsole.GetConsoleWindow<ObjectTreeWindow>("Object Tree");
        if (_objTree == null)
        {
            GD.PrintErr("[RuntimeConsole]: Object Tree window not found");
            return;
        }

        _objName = GetNode<Label>("%ObjectName");
        _objRID = GetNode<Label>("%ObjectRID");
        _objProperties = GetNode<VBoxContainer>("%PropertyContainer");

        _objTree.NodeSelected += OnNodeSelected;
    }

    private void OnNodeSelected(Node node)
    {
        ClearMembers();
        _obj = node;
        _objName.Text = node.Name;
        _objRID.Text = $"{node.Name}<#{node.GetInstanceId()}>";
        CreateProperties(node);
    }

    private void CreateProperties(Node node)
    {
        var properties = node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        
        foreach (var propertyInfo in properties)
        {
            // 跳过不可读属性
            if (!propertyInfo.CanRead)
                continue;
                
            // 创建对应的属性编辑器
            var editor = PropertyEditorFactory.Create(propertyInfo.PropertyType);
            editor.Name = propertyInfo.Name;
            _objProperties.AddChild(editor);

            // 设置编辑器是否可编辑（仅当属性可写时）
            bool isEditable = propertyInfo.CanWrite;
            editor.SetEditable(isEditable);
            
            // 如果属性可写，订阅值变更事件
            if (isEditable)
            {
                editor.ValueChanged += (value) =>
                {
                    try
                    {                        
                        propertyInfo.SetValue(_obj, value);
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Failed to set property '{propertyInfo.Name}': {ex.Message}");
                    }
                };
            }
                        
            var value = propertyInfo.GetValue(node);
            editor.SetProperty(propertyInfo.Name, propertyInfo.PropertyType, value);
        }
    }

    private void ClearMembers()
    {
        foreach (var child in _objProperties.GetChildren())
        {
            child.QueueFree();
        }
    }
}
