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
    
    private Tree _tree;
    private Button _refreshButton;
    private Theme _editorIcons;
    private ObjectTreeSearchBox _searchBox;
    public override void _Ready()
    {
        Size = (Vector2I)GetTree().Root.GetViewport().GetVisibleRect().Size / 2;

        _tree = GetNode<Tree>("%SceneTree");
        _refreshButton = GetNode<Button>("%RefreshButton");
        _searchBox = GetNode<ObjectTreeSearchBox>("%SearchBox");

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

        _searchBox.MatchItemFocused += OnMatchItemFocused;
        _tree.ItemSelected += OnItemSelected;
        _refreshButton.Pressed += FillObjectTree;
    }

    private void FillObjectTree()
    {
        _tree.Clear();
        var rootNode = GetTree().Root;

        if (!IsNodeValid(rootNode)) return;

        var rootItem = _tree.CreateItem();
        SetItemContent(rootItem, 0, rootNode, GetIcon(rootNode));
        CreateChildItem(rootNode, rootItem);
        _searchBox.SetRoot(_tree.GetRoot());
        _tree.Visible = true;
    }

    private void CreateChildItem(Node parentNode, TreeItem parent)
    {

        foreach (var child in parentNode.GetChildren().Cast<Node>().Where(IsNodeValid))
        {
            if (IsHiddenInTree(child))
                continue;

            var item = parent.CreateChild();
            SetItemContent(item, 0, child, GetIcon(child));
            CreateChildItem(child, item);
        }
    }
    private bool IsNodeValid(Node node)
    {
        return node != null && IsInstanceValid(node);
    }
    private void SetItemContent(TreeItem item, int column, Node meta, Texture2D icon)
    {
        item.SetMetadata(column, meta);
        item.SetText(column, meta.Name);
        item.SetCustomColor(column, Colors.White);
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
        else
        {
            _tree.Visible = false;
            CallDeferred(MethodName.FillObjectTree);            
        }
    }

    /// <summary>
    /// 处理搜索匹配项聚焦事件
    /// </summary>
    /// <param name="matchItem">匹配的树项</param>
    private void OnMatchItemFocused(TreeItem matchItem)
    {
        if (matchItem == null) return;
        
        // 选中指定项和第一列（0）
        _tree.SetSelected(matchItem, 0);

        // 滚动到该项
        _tree.ScrollToItem(matchItem, true);
    }

}

/// <summary>
/// 阻止该类在运行时对象树中显示。
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HideInObjectTreeAttribute : Attribute { }