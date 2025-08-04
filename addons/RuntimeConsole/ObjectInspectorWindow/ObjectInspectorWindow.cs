using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RuntimeConsole;

public partial class ObjectInspectorWindow : Window
{
    private ObjectTreeWindow _objTree;
    private Label _objName;
    private Label _objRID;
    private TabContainer _selectedObjectsContainer;
    private object _obj;
    private PackedScene _memberPanel = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/ObjectMemberPanel.tscn");
    private Stack<object> _selectedObjects = [];
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
        _selectedObjectsContainer = GetNode<TabContainer>("%SelectedObjects");

        CloseRequested += Hide;
        _objTree.NodeSelected += OnNodeSelected;
        _selectedObjectsContainer.TabChanged += OnTabChanged;
    }

    private void OnTabChanged(long tabIdx)
    {
        // 从子级切换到父级
        if (tabIdx + 1 < _selectedObjectsContainer.GetTabCount())
        {
            for (int i = _selectedObjectsContainer.GetTabCount() - 1; i > tabIdx; i--)
            {
                _selectedObjectsContainer.GetChild(i).QueueFree();
                _selectedObjects.Pop();
                SetTitle(_selectedObjects.Peek(), _selectedObjectsContainer.GetTabTitle(i - 1));
            }
        }
    }

    private void OnNodeSelected(Node node)
    {
        ClearObjects();
        ShowNodeMembers(node, string.Empty, node.Name);
    }



    // 显示复杂对象的成员
    private void OnCreateNewPanelRequested(object obj, string member)
    {        
        if (obj is Node node)
        {
            ShowNodeMembers(node, member, node.Name);
        }
        else
        {
            ShowObjectMembers(obj, member);
        }
        _selectedObjectsContainer.SelectNextAvailable();
    }

    private void ShowObjectMembers(object obj, string displayText)
    {
        SetTitle(obj, displayText);

        CreateNewPanel(obj, displayText, displayText);

    }

    private void SetTitle(object obj, string displayText)
    {
        if (obj == null)
        {
            _objName.Text = "null";
            _objRID.Text = "<null>";
            return;
        }

        _obj = obj;

        _objName.Text = displayText;
        _objRID.Text = obj is Node node
            ? $"{node.GetClass()}<#{node.GetInstanceId()}>" 
            : $"{obj.GetType().FullName} : {obj}";
    }


    private void CreateNewPanel(object obj, string parentProperty, string displayText)
    {

        var panel = _memberPanel.Instantiate<ObjectMemberPanel>();
        panel.Name = displayText;


        panel.CreateNewPanelRequested += OnCreateNewPanelRequested;

        if (_selectedObjects.Count > 0)
        {
            panel.SetParent(_selectedObjects.Peek(), parentProperty);
        }

        panel.SetObject(obj,
            new PropertyProvider(),
            new FieldProvider()
        );

        _selectedObjectsContainer.AddChild(panel);
        _selectedObjects.Push(obj);
    }

    // 显示Node成员

    private void ShowNodeMembers(Node node, string parentMember, string displayText)
    {
        SetTitle(node, node.Name);

        CreateNewPanel(node, parentMember, displayText);
    }

    private void ClearObjects()
    {
        foreach (var child in _selectedObjectsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        _selectedObjects.Clear();
    }
}
#nullable enable
/// <summary>
/// 指示在运行时对象检查器中显示字段或属性，并可指定自定义名称。
/// </summary>
/// <param name="displayName">用于替代字段或属性默认名称的显示名称（可选）。</param>
/// <remarks>
/// 默认会显示所有公共实例成员，无需显式标记此特性，除非需要设置显示名称。<br/>
/// 若成员为非公共或静态成员，且所在类已使用 <see cref="ExtendedInspectorAttribute"/> 标记，则标记此特性的成员也会被显示。<br/>
/// 未设置 <paramref name="displayName"/> 时，使用成员原始名称。
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ShowInInspectorAttribute(string? displayName = null) : Attribute
{
    public string? DisplayName { get; } = displayName;
}

/// <summary>
/// 指示该类的静态和非公共成员可在运行时对象检查器中显示。
/// </summary>
/// <param name="includeStatic">是否包含静态成员（默认：true）。</param>
/// <param name="includeNonPublic">是否包含非公共成员（默认：true）。</param>
[AttributeUsage(AttributeTargets.Class)]
public class ExtendedInspectorAttribute(bool includeStatic = true, bool includeNonPublic = true) : Attribute
{
    public bool IncludeStatic { get; } = includeStatic;
    public bool IncludeNonPublic { get; } = includeNonPublic;
}

/// <summary>
/// 阻止字段或属性在运行时对象检查器中显示。
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenInInspectorAttribute : Attribute { }