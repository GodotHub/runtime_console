using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

public partial class ObjectTreeSearchBox : HBoxContainer
{
    public event Action<TreeItem> MatchItemFocused;

    private LineEdit _searchBox;
    private Button _nextMatchButton;
    private Button _previousMatchButton;
    private string _searchTerm = string.Empty;
    private readonly List<TreeItem> _matchedItems = new();
    private int _currentMatchIndex = -1;
    private TreeItem _root;
    private TreeItem _lastHighlightedItem;
    private Color _highlightColor = new Color(0.9f, 0.7f, 0.1f); // 黄色
    private Color _defaultColor = new Color(1, 1, 1); // 白色默认

    public override void _Ready()
    {
        _searchBox = GetNode<LineEdit>("%LineEdit");
        _nextMatchButton = GetNode<Button>("%NextMatchButton");
        _previousMatchButton = GetNode<Button>("%PreviousMatchButton");

        _searchBox.TextChanged += OnSearchTextChanged;
        _nextMatchButton.Pressed += OnNextMatchButtonPressed;
        _previousMatchButton.Pressed += OnPreviousMatchButtonPressed;
        
        UpdateButtonStates();
    }

    private void OnPreviousMatchButtonPressed()
    {        
        NavigateToMatch(-1);
    }


    private void OnNextMatchButtonPressed()
    {
        NavigateToMatch(1);        
    }


    /// <summary>
    /// 设置搜索的根节点
    /// </summary>
    /// <param name="root">树的根节点</param>
    public void SetRoot(TreeItem root)
    {
        _root = root;
        // 当根节点改变时，如果已有搜索词，重新执行搜索
        if (!string.IsNullOrEmpty(_searchTerm) && _root != null)
        {
            PerformSearch(_searchTerm);
        }
    }
    
    private void OnSearchTextChanged(string newText)
    {
        _searchTerm = newText?.Trim() ?? string.Empty;
        
        if (_root == null) 
        {
            Reset();
            UpdateButtonStates();
            return;
        }

        PerformSearch(_searchTerm);
    }
    
    private void PerformSearch(string searchTerm)
    {
        Reset();

        if (string.IsNullOrEmpty(searchTerm))
        {
            UpdateButtonStates();
            return;
        }

        // 递归搜索树，收集匹配项
        CollectMatches(_root, searchTerm, _matchedItems);

        // 如果有匹配项，跳转到第一个匹配项
        if (_matchedItems.Count > 0)
        {
            _currentMatchIndex = 0;
            var matchItem = _matchedItems[_currentMatchIndex];
            FocusAndHighlightMatch(matchItem);
        }
        
        UpdateButtonStates();
    }

    private void CollectMatches(TreeItem item, string searchTerm, List<TreeItem> matches)
    {
        if (item == null) return;

        var name = item.GetText(0);
        // 模糊搜索（包含）
        if (name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            matches.Add(item);

        // 递归搜索子项
        for (int i = 0; i < item.GetChildCount(); i++)
        {
            CollectMatches(item.GetChild(i), searchTerm, matches);
        }
    }
    private void NavigateToMatch(int direction)
    {
        if (_matchedItems.Count == 0) return;

        _currentMatchIndex += direction;

        // 循环导航
        if (_currentMatchIndex >= _matchedItems.Count)
            _currentMatchIndex = 0;
        else if (_currentMatchIndex < 0)
            _currentMatchIndex = _matchedItems.Count - 1;


        var item = _matchedItems[_currentMatchIndex];     
        // 高亮显示匹配项
        FocusAndHighlightMatch(item);

        UpdateButtonStates();
    }
    
    /// <summary>
    /// 聚焦并高亮显示匹配项
    /// </summary>
    /// <param name="matchItem">要聚焦的树项</param>
    private void FocusAndHighlightMatch(TreeItem matchItem)
    {
        if (matchItem == null) return;

        // 重置上一个高亮项的颜色
        if (_lastHighlightedItem != null)
        {
            SetItemColor(_lastHighlightedItem, _defaultColor);
        }

        // 展开父节点保证可见
        TreeItem parent = matchItem.GetParent();
        while (parent != null)
        {
            parent.SetCollapsedRecursive(false);
            parent = parent.GetParent();
        }

        // 高亮当前项
        SetItemColor(matchItem, _highlightColor);
        _lastHighlightedItem = matchItem;

        // 触发聚焦事件
        MatchItemFocused?.Invoke(matchItem);
    }
    
    /// <summary>
    /// 设置树项的颜色
    /// </summary>
    /// <param name="item">树项</param>
    /// <param name="color">颜色</param>
    private void SetItemColor(TreeItem item, Color color)
    {
        if (item == null) return;
        item.SetCustomColor(0, color);
    }

    private void Reset()
    {
        // 清除之前的高亮
        if (_lastHighlightedItem != null)
        {
            SetItemColor(_lastHighlightedItem, _defaultColor);
            _lastHighlightedItem = null;
        }
        
        _matchedItems.Clear();
        _currentMatchIndex = -1;
    }
    
    private void UpdateButtonStates()
    {
        // 更新按钮文本
        bool hasMatches = _matchedItems.Count > 0;

        _previousMatchButton.Disabled = !hasMatches || _matchedItems.Count <= 1;
        _nextMatchButton.Disabled = !hasMatches || _matchedItems.Count <= 1;

        string buttonText = hasMatches ? $"{_currentMatchIndex + 1}/{_matchedItems.Count}" : "";
        _previousMatchButton.Text = $"< {buttonText}";
        _nextMatchButton.Text = $"> {buttonText}";
    }
}