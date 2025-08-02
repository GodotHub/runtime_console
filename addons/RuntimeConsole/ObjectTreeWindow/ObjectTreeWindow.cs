using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class ObjectTreeWindow : Window
{

    [Signal]
    public delegate void NodeSelectedEventHandler(Node node);

    private List<Type> _enumTypes = new();
    private Tree _tree;
    private LineEdit _searchBox;
    private Button _nextMatchButton;
    private Button _previousMatchButton;
    private Theme _editorIcons;
    private string _searchTerm = string.Empty;

    private Color _highlightColor = new Color(0.9f, 0.7f, 0.1f); // 黄色
    private Color _defaultColor = new Color(1, 1, 1); // 白色默认
    private readonly List<TreeItem> _matchedItems = new();
    private int _currentMatchIndex = -1;
    private TreeItem _lastMatch = null;
    public override void _Ready()
    {
        Size = (Vector2I)GetTree().Root.GetViewport().GetVisibleRect().Size / 2;

        _tree = GetNode<Tree>("%SceneTree");
        _searchBox = GetNode<LineEdit>("%SearchBox");
        _nextMatchButton = GetNode<Button>("%NextMatchButton");
        _previousMatchButton = GetNode<Button>("%PreviousMatchButton");

        _editorIcons = Console.GameConsole.GetConfig().EditorIconsTheme;
        if (_editorIcons == null)
        {
            GD.PrintErr("[RuntimeConsole]: Editor icons theme resource not found. The Object Inspector will not be show node icons.");
        }

        CloseRequested += Hide;
        VisibilityChanged += () =>
        {
            if (Visible) // 显示窗口时刷新树
            {
                FillObjectTree();
            }
        };

        _searchBox.TextChanged += OnSearchTextChanged;
        _nextMatchButton.Pressed += OnNextMatch;
        _previousMatchButton.Pressed += OnPreviousMatch;
        _tree.ItemSelected += OnItemSelected;
    }

    private void FillObjectTree()
    {
        _tree.Clear();
        var rootNode = GetTree().Root;
        var rootItem = _tree.CreateItem();

        SetItemContent(rootItem, 0, rootNode, GetIcon(rootNode));
        CreateChildItem(rootNode, rootItem);
    }

    private void CreateChildItem(Node rootNode, TreeItem parent)
    {

        foreach (var child in rootNode.GetChildren())
        {
            if (IsHiddenInTree(child))
                continue;

            var item = parent.CreateChild();
            SetItemContent(item, 0, child, GetIcon(child));
            CreateChildItem(child, item);
        }
    }

    private void SetItemContent(TreeItem item, int column, Node meta, Texture2D icon)
    {
        item.SetMetadata(column, meta);
        item.SetText(column, meta.Name);
        SetDefaultColor(item);
        if (icon != null)
            item.SetIcon(column, icon);
    }

    private Texture2D GetIcon(Node node)
    {
        return _editorIcons?.GetIcon(node.GetClass(), "EditorIcons");
    }

    private bool IsHiddenInTree(Node node)
    {
        var script = node.GetScript();

        if (script.Obj != null)
        {
            if (script.Obj is GDScript gdScript)
            {
                var sourceCode = gdScript.SourceCode;

                // 源代码为空，默认显示在树中
                if (string.IsNullOrEmpty(sourceCode))
                    return false;

                // 需求第一行包含关键词，来将节点隐藏在树中
                var firstLine = sourceCode.Split('\n', '\r').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
                if (firstLine != null && firstLine.Trim().StartsWith("# @hide_in_object_tree"))
                    return true;
            }

            if (script.Obj is CSharpScript)
            {
                // 反射拿attribute
                var attribute = node.GetType().GetCustomAttribute(typeof(HideInObjectTreeAttribute));
                if (attribute != null)
                    return true;
            }
        }

        // 节点没有挂载脚本，默认显示在树中
        return false;
    }

    private void OnItemSelected()
    {
        var selectedItem = _tree.GetSelected();
        var meta = selectedItem.GetMetadata(0).As<Node>();
        if (meta != null)
        {
            EmitSignalNodeSelected(meta);
        }
    }

#region 搜索功能
    private void OnSearchTextChanged(string newText)
    {
        _searchTerm = newText.Trim().ToLowerInvariant(); // 转换成小写
        _matchedItems.Clear();
        _currentMatchIndex = -1;

        if (string.IsNullOrEmpty(_searchTerm))
            return;

        // 递归搜索树，收集匹配项
        CollectMatches(_tree.GetRoot(), _matchedItems);

        // 如果有匹配项，跳转到第一个匹配项
        if (_matchedItems.Count > 0)
        {
            _currentMatchIndex = 0;
            FocusOnMatch(_matchedItems[_currentMatchIndex]);
        }
    }

    private void CollectMatches(TreeItem item, List<TreeItem> matches)
    {
        if (item == null) return;

        var name = item.GetText(0).ToLowerInvariant(); // 转换成小写
        // 模糊搜索（包含）
        if (name.Contains(_searchTerm))
            matches.Add(item);

        // 递归搜索子项
        for (int i = 0; i < item.GetChildCount(); i++)
        {
            CollectMatches(item.GetChild(i), matches);
        }
    }


    private void SetDefaultColor(TreeItem item)
    {
        if (item == null) return;

        item.SetCustomColor(0, _defaultColor); // 第一列
    }

    private void HighlightItem(TreeItem item)
    {
        if (item == null) return;
        item.SetCustomColor(0, _highlightColor);
        item.SetCustomColor(1, _highlightColor);
        item.SetCustomColor(2, _highlightColor);
    }


    private void FocusOnMatch(TreeItem match)
    {
        if (match == null) return;

        // 重置颜色
        if (_lastMatch != null)
        {
            SetDefaultColor(_lastMatch);
        }

        // 展开父节点保证可见
        TreeItem parent = match.GetParent();
        while (parent != null)
        {
            parent.SetCollapsedRecursive(false);
            parent = parent.GetParent();
        }

        // 选中指定项和第一列（0）
        _tree.SetSelected(match, 0);

        // 滚动到该项
        _tree.ScrollToItem(match, true);

        // 高亮文本
        HighlightItem(match);

        // 更新上一个匹配项引用
        _lastMatch = match;
    }

    // 下一个匹配项
    private void OnNextMatch()
    {
        if (_matchedItems.Count == 0) return;

        _currentMatchIndex = (_currentMatchIndex + 1) % _matchedItems.Count;
        FocusOnMatch(_matchedItems[_currentMatchIndex]);
    }

    // 上一个匹配项
    private void OnPreviousMatch()
    {
        if (_matchedItems.Count == 0) return;

        _currentMatchIndex--;
        if (_currentMatchIndex < 0)
            _currentMatchIndex = _matchedItems.Count - 1;

        FocusOnMatch(_matchedItems[_currentMatchIndex]);
    }
#endregion

}
#nullable enable

/// <summary>
/// 指示在运行时对象检查器中显示字段或属性，并可指定自定义名称。
/// </summary>
/// <param name="displayName">用于替代字段或属性默认名称的显示名称（可选）。</param>
/// <remarks>
/// 默认会显示所有公共实例成员，无需显式标记此特性，除非需要设置显示名称。<br/>
/// 若成员为非公共或静态成员，且所在类已使用 <see cref="InspectableObjectAttribute"/> 标记，则标记此特性的成员也会被显示。<br/>
/// 未设置 <paramref name="displayName"/> 时，使用成员原始名称。
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InspectableAttribute(string? displayName = null) : Attribute
{
    public string? DisplayName { get; } = displayName;
}

/// <summary>
/// 指示该类的静态和非公共成员可在运行时对象检查器中显示。
/// </summary>
/// <param name="includeStatic">是否包含静态成员（默认：true）。</param>
/// <param name="includeNonPublic">是否包含非公共成员（默认：true）。</param>
[AttributeUsage(AttributeTargets.Class)]
public class InspectableObjectAttribute(bool includeStatic = true, bool includeNonPublic = true) : Attribute
{
    public bool IncludeStatic { get; } = includeStatic;
    public bool IncludeNonPublic { get; } = includeNonPublic;
}

/// <summary>
/// 阻止字段或属性在运行时对象检查器中显示。
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HiddenInInspectorAttribute : Attribute { }

/// <summary>
/// 阻止该类在运行时对象树中显示。
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HideInObjectTreeAttribute : Attribute { }