using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RuntimeConsole;

public partial class ObjectInspectorWindow : Window
{

    private ObjectTreeWindow _objTree;
    private Label _objName;
    private Label _objRID;
    private Button _showClipboardButton;
    private TabContainer _selectedObjectsContainer;
    private Button _pinButton;
    private static readonly PackedScene _memberPanel = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/ObjectMemberPanel.tscn");
    private readonly Stack<object> _selectedObjects = [];
    private Callable _clearObjectsCallable;

    public override void _Ready()
    {
        _clearObjectsCallable = new Callable(this, MethodName.ClearObjects);
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
        _showClipboardButton = GetNode<Button>("%ClipboardButton");
        _pinButton = GetNode<Button>("%PinButton");

        CloseRequested += Hide;
        _objTree.NodeSelected += OnNodeSelected;
        _selectedObjectsContainer.TabChanged += OnTabChanged;
        _showClipboardButton.Pressed += OnClipboardButtonPressed;
        _pinButton.Pressed += OnPinButtonPressed;
    }

    private void OnClipboardButtonPressed()
    {
        Clipboard.Instance.Show();
    }

    private void OnPinButtonPressed()
    {
        var obj = _selectedObjects.Peek();
        if (obj != null)
        {
            Clipboard.Instance.AddEntry(obj);
        }    
    }

    private void OnTabChanged(long tabIdx)
    {
        // 从子级切换到父级
        if (tabIdx + 1 < _selectedObjectsContainer.GetTabCount())
        {
            for (int i = _selectedObjectsContainer.GetTabCount() - 1; i > tabIdx; i--)
            {
                if (_selectedObjects.Count > 0 && IsObjectInvalid(_selectedObjects.Peek()))
                {
                    _selectedObjectsContainer.GetChild(i).QueueFree();
                    _selectedObjects.Pop();
                    continue;
                }
                _selectedObjectsContainer.GetChild(i).QueueFree();
                _selectedObjects.Pop();
                SetTitle(_selectedObjects.Peek(), _selectedObjectsContainer.GetTabTitle(i - 1));
            }
        }
    }

    private bool IsObjectInvalid(object obj)
    {
        if (obj == null)
            return true;

        if (obj is Node node && !IsInstanceValid(node))
            return true;

        return false;
    }


    private async void OnNodeSelected(Node node)
    {
        ClearObjects();
        
        if (!node.IsConnected(Node.SignalName.TreeExiting, _clearObjectsCallable))
            node.Connect(Node.SignalName.TreeExiting, _clearObjectsCallable);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        ShowNodeMembers(node, string.Empty, node.Name);
    }



    // 显示复杂对象的成员
    private async void OnCreateNewPanelRequested(PropertyEditorBase sender, object obj, object[] context)
    {
        if (obj is Node node)
        {
            ShowNodeMembers(node, sender.MemberName, string.IsNullOrEmpty(node.Name) ? sender.MemberName : node.Name);
        }
        else if (sender is CollectionPropertyEditor)
        {
            ShowObjectMembers(obj, sender.MemberName, context);
        }
        else
        {
            ShowObjectMembers(obj, sender.MemberName);
        }

        // 等待一帧，等面板创建好
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
       
        _selectedObjectsContainer.SelectNextAvailable();
        
        var lastIndex = _selectedObjectsContainer.GetChildCount() - 1;
        if (lastIndex >= 0)
        {
            if (_selectedObjectsContainer.GetChild(lastIndex) is ObjectMemberPanel panel)
            {
                panel.Created += () =>
                {
                    ((IExpendObjectRequester)sender).OnPanelCreated(panel);
                };
            }
        }
    }

    private void ShowObjectMembers(object obj, string displayText, object[] context = null)
    {
        SetTitle(obj, displayText);

        CreateNewPanel(obj, displayText, displayText, context);

    }

    private void SetTitle(object obj, string displayText)
    {
        if (IsObjectInvalid(obj))
        {
            _objName.Text = "[Released object]";
            _objRID.Text = "<null>";
            return;
        }

        _objName.Text = displayText;
        _objRID.Text = obj is Node node
            ? $"{node.GetClass()}<#{node.GetInstanceId()}>" 
            : $"{obj.GetType()} : {obj}";
    }


    private async void CreateNewPanel(object obj, string parentProperty, string displayText, object[] context = null, bool push = true)
    {

        var panel = _memberPanel.Instantiate<ObjectMemberPanel>();
        panel.Name = displayText;


        panel.CreateNewPanelRequested += OnCreateNewPanelRequested;

        if (_selectedObjects.Count > 0)
        {
            panel.SetParent(parentProperty);
        }

        // 等一帧，以防有同名面板没有释放
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _selectedObjectsContainer.AddChild(panel);

        if (push)
            _selectedObjects.Push(obj);

        panel.SetObject(obj,
            context,
            new GDScriptPropertyProvider(),
            new PropertyProvider(),
            new FieldProvider(),
            new GDScriptMethodProvider(),
            new MethodProvider(),
            new GDScriptSignalProvider(),
            new EventProvider()
        );        
        
        panel.RefreshRequested += OnRefreshRequested;
    }

    private void OnRefreshRequested(ObjectMemberPanel panel)
    {
        if (_selectedObjects.Count == 0) return;
        
        var currentObj = _selectedObjects.Peek();
        
        if (IsObjectInvalid(currentObj))
        {
            _selectedObjects.Pop();
            panel.QueueFree();
            return;
        }

        var context = panel.Context;

        panel.SetObject(currentObj,
            context,
            new GDScriptPropertyProvider(),
            new PropertyProvider(),
            new FieldProvider(),
            new GDScriptMethodProvider(),
            new MethodProvider(),
            new GDScriptSignalProvider(),
            new EventProvider()
        );

        SetTitle(IsObjectInvalid(currentObj) ? null : currentObj, panel.Name);
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
        _objName.Text = "Please select an object from the object tree.";
        _objRID.Text = "";
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