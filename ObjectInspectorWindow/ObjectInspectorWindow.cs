using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class ObjectInspectorWindow : Window
{
    private Tree _tree;
    private LineEdit _searchBox;
    private Button _nextMatchButton;
    private Button _previousMatchButton;
    private string _searchTerm = string.Empty;
#nullable enable

    public override void _Ready()
    {
        _tree = GetNode<Tree>("%ObjectTree");
        _searchBox = GetNode<LineEdit>("%SearchBox");   
        _nextMatchButton = GetNode<Button>("%NextMatchButton"); 
        _previousMatchButton = GetNode<Button>("%PreviousMatchButton");    
        CloseRequested += Hide;
        VisibilityChanged += () =>
        {
            if (Visible) // 显示窗口时刷新树
            {
                InspectAllSceneObjects();
            }
        };
        _searchBox.TextChanged += OnSearchTextChanged;
        _nextMatchButton.Pressed += OnNextMatch;
        _previousMatchButton.Pressed += OnPreviousMatch;

        _tree.Columns = 3; // 设置列数为3
        _tree.SetColumnTitle(0, "Name"); // 设置标题
        _tree.SetColumnTitle(1, "Type");
        _tree.SetColumnTitle(2, "Value");
        _tree.SetColumnTitlesVisible(true); // 显示标题

    }

    private readonly List<TreeItem> _matchedItems = new();
    private int _currentMatchIndex = -1;

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
        var typeName = item.GetText(1).ToLowerInvariant();
        var value = item.GetText(2).ToLowerInvariant();
        // 模糊搜索（包含）
        if (name.Contains(_searchTerm) || typeName.Contains(_searchTerm) || value.Contains(_searchTerm))
            matches.Add(item);

        // 递归搜索子项
        for (int i = 0; i < item.GetChildCount(); i++)
        {
            CollectMatches(item.GetChild(i), matches);
        }
    }
    private Color _highlightColor = new Color(0.9f, 0.7f, 0.1f); // 黄色
    private Color _defaultColor = new Color(1, 1, 1); // 白色默认

    private void SetDefaultColor(TreeItem item)
    {
        if (item == null) return;

        item.SetCustomColor(0, _defaultColor); // 第一列
        item.SetCustomColor(1, _defaultColor); // 第二列
        item.SetCustomColor(2, _defaultColor); // 第三列，如果有

    }

    private void HighlightItem(TreeItem item)
    {
        if (item == null) return;
        item.SetCustomColor(0, _highlightColor);
        item.SetCustomColor(1, _highlightColor);
        item.SetCustomColor(2, _highlightColor);
    }

    private TreeItem? _lastMatch = null;

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


    // 开始从根节点构建树
    public void InspectAllSceneObjects()
    {
        _tree.Clear();

        var rootNode = GetTree().Root;
        if (rootNode == null) return;

        var rootItem = _tree.CreateItem();
        SetDefaultColor(rootItem);
        ShowNodeHierarchy(rootItem, rootNode);
    }

    // 递归显示节点
    private void ShowNodeHierarchy(TreeItem parentItem, Node node)
    {
        var item = _tree.CreateItem(parentItem);
        SetDefaultColor(item);
        item.SetText(0, node.Name);
        item.SetText(1, node.GetType().Name);

        FillObjectTree(item, node, "self", new());

        foreach (Node child in node.GetChildren())
        {
            var childType = child.GetType();
            // 检查是否需要隐藏该类型
            if (childType.GetCustomAttribute<HideInObjectTreeAttribute>() != null)
                continue;

            ShowNodeHierarchy(item, child);
        }
    }

    // 构建树
    private void FillObjectTree(TreeItem parent, object? obj, string label, HashSet<object> visited)
    {
        if (obj == null)
        {
            var item = _tree.CreateItem(parent);
            SetDefaultColor(item);
            item.SetText(0, label);
            item.SetText(1, "null");
            return;
        }

        Type type = obj.GetType();

        // 检查是否需要隐藏该类型
        if (type.GetCustomAttribute<HideInObjectTreeAttribute>() != null)
            return;

        // 创建TreeItem
        var node = _tree.CreateItem(parent);
        SetDefaultColor(node);
        node.SetText(0, label);
        node.SetText(1, type.Name);

        if (IsCyclicReference(obj, type, visited)) // 防止循环引用炸掉
        {
            node.SetText(2, "[Cyclic]");
            return;
        }

        visited.Add(obj);

        // NodePath和StringName的特殊处理
        if (obj is NodePath nodePath)
        {
            node.SetText(2, nodePath.IsEmpty ? "[Empty]" : nodePath.ToString());
            return;
        }
        if (obj is StringName stringName)
        {
            node.SetText(2, stringName.IsEmpty ? "[Empty]" : stringName.ToString());
            return;
        }

        var attribute = type.GetCustomAttribute<InspectableObjectAttribute>();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (attribute != null)
        {
            if (attribute.IncludeNonPublic) flags |= BindingFlags.NonPublic;
            if (attribute.IncludeStatic) flags |= BindingFlags.Static;
        }


        if (obj is IEnumerable enumerable && type != typeof(string))
        {
            int index = 0;
            foreach (var element in enumerable)
            {
                FillObjectTree(node, element, $"[{index}]", visited);
                index++;
            }
        }
        else if (!type.IsPrimitive && type != typeof(string))
        {
            foreach (var prop in type.GetProperties(flags))
            {
                if (prop.GetIndexParameters().Length > 0) continue; // 跳过索引器属性
                var getter = prop.GetGetMethod(true); // 加 true 获取私有 getter
                bool isStatic = getter?.IsStatic == true;
                bool isNonPublic = getter != null && !getter.IsPublic;
                var displayName = prop.Name;
                var inspectableAttr = prop.GetCustomAttribute<InspectableAttribute>();

                // 如果属性是私有或静态，且没有标记 Inspectable，就跳过
                if ((isNonPublic || isStatic) && inspectableAttr == null)
                    continue;

                // 标记为 HiddenInInspector 的属性也跳过
                if (prop.GetCustomAttribute<HiddenInInspectorAttribute>() != null)
                    continue;

                if (inspectableAttr != null && inspectableAttr.DisplayName != null)
                    displayName = inspectableAttr.DisplayName;

                object? value;
                try { value = prop.GetValue(obj); } catch { continue; }

                FillObjectTree(node, value, displayName, visited);
            }

            foreach (var field in type.GetFields(flags))
            {
                bool isNonPublic = !field.IsPublic;
                bool isStatic = field.IsStatic;
                var inspectableAttr = field.GetCustomAttribute<InspectableAttribute>();

                if ((isNonPublic || isStatic) && inspectableAttr == null)
                    continue; // 跳过未标记的非 public 或静态字段

                // 跳过标记为 HiddenInInspector 的字段
                if (field.GetCustomAttribute<HiddenInInspectorAttribute>() != null)
                    continue;

                var displayName = field.Name;

                if (inspectableAttr != null && inspectableAttr.DisplayName != null)
                    displayName = inspectableAttr.DisplayName;

                object? value = field.GetValue(obj);

                FillObjectTree(node, value, displayName, visited);
            }
        }
        else
        {
            node.SetText(2, obj.ToString());
        }
    }

    // 排除会造成无限递归的引用
    private bool IsCyclicReference(object obj, Type type, HashSet<object> visited)
    {
        return visited.Contains(obj)
            && type.IsClass
            && !type.IsValueType
            && !type.IsPrimitive
            && type != typeof(string)
            && type != typeof(NodePath)
            && type != typeof(StringName);
    }


}

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
/// 阻止该类在运行时对象检查器中显示。
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HideInObjectTreeAttribute : Attribute { }


public static class NodeExtensions
{
    /// <summary>
    /// 递归获取该节点的所有子节点
    /// </summary>
    /// <param name="node">需要获取子节点的父级节点</param>
    /// <returns>一个列表，包含该节点的所有子节点</returns>
    public static List<Node> GetChildrenRecursive(this Node node)
    {
        var result = new List<Node>();
        foreach (var child in node.GetChildren())
        {
            result.Add(child);
            result.AddRange(child.GetChildrenRecursive());
        }
        return result;
    }
}
