using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RuntimeConsole;

public partial class ObjectMemberPanelSearchBox : HBoxContainer
{
    private LineEdit _searchBox;
    private Button _prevButton;
    private Button _nextButton;
    private List<PropertyEditorBase> _allProperties = [];
    private List<PropertyEditorBase> _allFields = [];
    private List<PropertyEditorBase> _allElements = [];
    private List<MethodEditor> _allMethods = [];
    private List<EventEditor> _allEvents = [];
    private List<PropertyEditorBase> _filteredProperties = [];
    private List<PropertyEditorBase> _filteredFields = [];
    private List<PropertyEditorBase> _filteredElements = [];
    private List<MethodEditor> _filteredMethods = [];
    private List<EventEditor> _filteredEvents = [];

    private int _currentMatchIndex;
    private string _currentSearchTerm = string.Empty;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }
    }

    public void SetMemberEditors(
        List<PropertyEditorBase> elements,
        List<PropertyEditorBase> properties,
        List<PropertyEditorBase> fields,
        List<MethodEditor> methods,
        List<EventEditor> events
    )
    {
        _allElements = elements ?? throw new ArgumentNullException(nameof(elements));
        _allProperties = properties ?? throw new ArgumentNullException(nameof(properties));
        _allFields = fields ?? throw new ArgumentNullException(nameof(fields));
        _allMethods = methods ?? throw new ArgumentNullException(nameof(methods));
        _allEvents = events ?? throw new ArgumentNullException(nameof(events));
        
        // 如果已有搜索词，重新执行搜索
        if (!string.IsNullOrEmpty(_currentSearchTerm))
        {
            PerformSearch(_currentSearchTerm);
        }
    }

    private void OnSceneInstantiated()
    {
        _searchBox = GetNode<LineEdit>("LineEdit");
        _prevButton = GetNode<Button>("PreviousMatchButton");
        _nextButton = GetNode<Button>("NextMatchButton");

        _searchBox.TextChanged += OnSearchTextChanged;
        _prevButton.Pressed += OnPreviousMatchButtonPressed;
        _nextButton.Pressed += OnNextMatchButtonPressed;

        // 初始化按钮状态
        _prevButton.Disabled = true;
        _nextButton.Disabled = true;
    }

    private void OnPreviousMatchButtonPressed()
    {
        NavigateToMatch(-1);
    }
    private void OnNextMatchButtonPressed()
    {
        NavigateToMatch(1);
    }


    private void OnSearchTextChanged(string newText)
    {
        _currentSearchTerm = newText ?? string.Empty;
        PerformSearch(_currentSearchTerm);
        UpdateSearchNavigationButtons();
    }

    private void Reset()
    {
        _filteredProperties.Clear();
        _filteredFields.Clear();
        _filteredElements.Clear();
        _filteredMethods.Clear();
        _filteredEvents.Clear();

        _currentMatchIndex = -1;

    }

    private void UpdateSearchNavigationButtons()
    {
        var matches = GetAllVisibleItems();
        bool hasMatches = matches.Count > 0;

        _prevButton.Disabled = !hasMatches || matches.Count <= 1;
        _nextButton.Disabled = !hasMatches || matches.Count <= 1;

        string buttonText = hasMatches ? $"{_currentMatchIndex + 1}/{matches.Count}" : "";
        _prevButton.Text = $"< {buttonText}";
        _nextButton.Text = $"> {buttonText}";
    }

    private void PerformSearch(string searchTerm)
    {
        Reset();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            // 如果搜索词为空，显示所有项
            ShowAllItems();
            return;
        }

        // 过滤属性
        _filteredProperties = FilterItems(_allProperties, searchTerm);
        _filteredFields = FilterItems(_allFields, searchTerm);
        _filteredElements = FilterItems(_allElements, searchTerm);
        _filteredMethods = FilterItems(_allMethods, searchTerm);
        _filteredEvents = FilterItems(_allEvents, searchTerm);

        // 更新显示
        UpdateVisibleItems();
    }

    private List<T> FilterItems<T>(IReadOnlyList<T> source, string searchTerm)
        where T : IMemberEditor
    {
        return source
            .Where(item => item.MemberName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void UpdateVisibleItems()
    {
        SetVisibility(_allProperties, _filteredProperties);
        SetVisibility(_allFields, _filteredFields);
        SetVisibility(_allElements, _filteredElements);
        SetVisibility(_allMethods, _filteredMethods);
        SetVisibility(_allEvents, _filteredEvents);
    }

    private void SetVisibility<T>(IReadOnlyList<T> allItems, List<T> filteredItems) where T : Control
    {
        foreach (var item in allItems)
        {
            item.Visible = filteredItems.Contains(item);
        }
    }

    private void ShowAllItems()
    {
        ShowItems(_allProperties);
        ShowItems(_allFields);
        ShowItems(_allElements);
        ShowItems(_allMethods);
        ShowItems(_allEvents);
    }

    private void ShowItems<T>(IReadOnlyList<T> items) where T : Control
    {
        foreach (var item in items)
        {
            item.Visible = true;
        }
    }
    
    private void NavigateToMatch(int direction)
    {
        var allMatches = GetAllVisibleItems();
        if (allMatches.Count == 0) return;

        _currentMatchIndex += direction;

        // 循环导航
        if (_currentMatchIndex >= allMatches.Count)
            _currentMatchIndex = 0;
        else if (_currentMatchIndex < 0)
            _currentMatchIndex = allMatches.Count - 1;


        var item = allMatches[_currentMatchIndex];     
        // 滚动到匹配项
        ScrollToItem(item);

        // 高亮显示匹配项
        HighlightMatchItem(item);

        UpdateSearchNavigationButtons();
    }
    private void ScrollToItem(Control item)
    {
        // 找到项目所在的容器
        var parent = item.GetParent().GetParent();
        if (parent is ScrollContainer scrollContainer)
        {
            // 计算滚动位置，留一些边距
            var itemPosition = item.GetPosition();
            scrollContainer.ScrollVertical = Math.Max(0, (int)itemPosition.Y - 30);
        }
    }
    private List<Control> GetAllVisibleItems()
    {
        var items = new List<Control>();

        AddVisibleItems(_filteredProperties, items);
        AddVisibleItems(_filteredFields, items);
        AddVisibleItems(_filteredElements, items);
        AddVisibleItems(_filteredMethods, items);
        AddVisibleItems(_filteredEvents, items);

        return items;
    }

    private void AddVisibleItems<T>(List<T> source, List<Control> target) where T : Control
    {
        foreach (var item in source)
        {
            if (item.Visible)
                target.Add(item);
        }
    }
    
    private void HighlightMatchItem(Control item)
    {
        // 移除之前项目的高亮
        RemoveAllHighlights();

        // 高亮当前项目
        item.Modulate = Colors.Yellow;
    }
    private void RemoveAllHighlights()
    {
        RemoveHighlights(_allProperties);
        RemoveHighlights(_allFields);
        RemoveHighlights(_allElements);
        RemoveHighlights(_allMethods);
        RemoveHighlights(_allEvents);
    }

    private void RemoveHighlights<T>(IReadOnlyList<T> items) where T : Control
    {
        foreach (var item in items)
        {
            item.Modulate = Colors.White;
        }
    }
}