using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class ObjectInspectorWindow : Window
{
    /// <summary>
    /// 是否展示 GDScript 中枚举属性的名称（如 "Error.OK"），否则仅显示枚举的数值（如 0）。
    /// <br/>
    /// 启用后将遍历 GodotSharp 中所有枚举类型以匹配内置枚举名，
    /// 对性能影响极小，但在极端情况下可能稍微增加初始化时间。
    /// </summary>
    [Export]
    public bool ShowGDScriptEnumName
    {
        get => _showGDScriptEnumName;
        set
        {
            if (_showGDScriptEnumName == value)
                return;

            _showGDScriptEnumName = value;

            if (value && _enumTypes.Count == 0)
            {
                _enumTypes = typeof(GodotObject).Assembly
                    .GetTypes()
                    .Where(t => t.IsEnum && t.Namespace == "Godot")
                    .ToList();
            }
        }
    }
    private bool _showGDScriptEnumName = false;

    /// <summary>
    /// 是否在检查器中显示GDScript对象的属性
    /// </summary>
    [Export] public bool ShowGDScriptObjectProperties { get; set; } = true;
    private List<Type> _enumTypes = new();
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

        Size = (Vector2I)GetTree().Root.GetViewport().GetVisibleRect().Size / 2; 
        _tree.Columns = 3; // 设置列数为3
        _tree.SetColumnTitle(0, "Name"); // 设置标题
        _tree.SetColumnTitle(1, "Type");
        _tree.SetColumnTitle(2, "Value");
        _tree.SetColumnTitlesVisible(true); // 显示标题

        if (ShowGDScriptEnumName)
        {
            _enumTypes = typeof(GodotObject).Assembly
                .GetTypes()
                .Where(t => t.IsEnum && t.Namespace == "Godot")
                .ToList();
        }

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
    private void InspectAllSceneObjects()
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
        if (ShowGDScriptObjectProperties && IsUsingGDScript(node))
            item.SetText(1, GetGDScriptClass(node));
        item.SetText(2, node.ToString());

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
    private void FillObjectTree(TreeItem parent, object? obj, string label, HashSet<object> visited, string gdscriptType = "")
    {
        if (obj == null)
        {
            var item = _tree.CreateItem(parent);
            SetDefaultColor(item);
            item.SetText(0, label);
            item.SetText(1, "null");
            item.SetText(2, "NULL");
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
            node.SetText(2, $"[Cyclic]:{obj}");
            return;
        }

        visited.Add(obj);

        // NodePath和StringName、Variant、Enum（C#枚举）的特殊处理
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
        if (obj is Variant v)
        {
            var inner = v.Obj; // 取出内部的C#对象
            var typeName = v.VariantType.ToString(); // 类型名

            node.SetText(1, $"Variant({typeName})");

            if (inner == null)
            {
                node.SetText(2, "null");
                return;
            }

            // 是否为未被访问的GD对象
            if (inner is GodotObject godotObj)
            {                
                if (!visited.Contains(godotObj))
                {
                    if (!string.IsNullOrEmpty(gdscriptType))
                        node.SetText(1, $"Variant({typeName})\nGDScript:{gdscriptType}");                    
                    node.SetText(2, godotObj.ToString());
                    FillObjectTree(node, godotObj, "Obj", visited, gdscriptType);
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(gdscriptType))
                        node.SetText(1, $"Variant({typeName})\nGDScript:{gdscriptType}");
                    node.SetText(2, $"[Cyclic]:{godotObj}");
                    return;
                }
            }

            // 如果是集合类型
            if (inner is IEnumerable gdEnumerable && inner.GetType() != typeof(string))
            {
                node.SetText(2, "");  // 空着，内容在子节点展开
                int index = 0;
                foreach (var element in gdEnumerable)
                {
                    FillObjectTree(node, element, $"[{index}]", visited);
                    index++;
                }
                return;
            }

            // 普通值直接显示字符串
            node.SetText(2, string.IsNullOrEmpty(inner.ToString()) ? "[Empty]" : inner.ToString());
            return;
        }
        if (obj is Enum enumVal)
        {
            var enumName = enumVal.ToString();
            var enumIntValue = Convert.ToInt64(enumVal);
            node.SetText(1, obj.GetType().Name);
            node.SetText(2, $"{enumName} ({enumIntValue})");
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
            node.SetText(2, obj.ToString());
            if (ShowGDScriptObjectProperties && IsUsingGDScript(obj))
            {
                // 只处理 GDScript 中自定义的属性
                var gdObject = (GodotObject)obj;
                var members = gdObject.GetPropertyList();
                if (string.IsNullOrEmpty(gdscriptType))
                    node.SetText(1, GetGDScriptClass(gdObject));
                else
                    node.SetText(1, gdscriptType);

                foreach (var prop in members)
                {
                    if (!prop.TryGetValue("usage", out var usage)) continue;

                    // usage 是一个 int 类型的标志位
                    // 用 GodotObject.PropertyUsageFlags 枚举来判断是否包含 SCRIPT_VARIABLE，代表是否是用户定义的脚本变量（而不是内建的）
                    if ((int)usage != 0 && ((int)usage & (int)PropertyUsageFlags.ScriptVariable) != 0)
                    {
                        if (!prop.TryGetValue("name", out var nameVariant)) continue;
                        var name = nameVariant.As<string>();
                        var variant = gdObject.Get(name);
                        var className = prop["class_name"].ToString();
                        var script = (GDScript)gdObject.GetScript();


                        if (variant.VariantType == Variant.Type.Object)
                        {
                            gdscriptType = className;
                            var valueObj = variant.AsGodotObject();
                            var objScript = (Script)valueObj.GetScript();
                            var path = valueObj == null ? null : objScript?.ResourcePath;

                            if (valueObj != null)
                            {
                                gdscriptType = GetGDScriptClass(valueObj, script);
                                if (objScript is CSharpScript)
                                {
                                    gdscriptType = "(CSharpScirpt)" + gdscriptType;
                                }
                            }


                        }

                        // GDScript枚举的特殊处理
                        // 没有显式指定为枚举类型时，这里会返回false
                        // usage是135168 也就是 PROPERTY_USAGE_SCRIPT_VARIABLE = 4096 | PROPERTY_USAGE_NIL_IS_VARIANT = 131072
                        // 如果显式指定为枚举类型，usage是 69632，也就是
                        // PROPERTY_USAGE_SCRIPT_VARIABLE = 4096 | PROPERTY_USAGE_CLASS_IS_ENUM = 65536
                        var isEnum = ((int)prop["usage"] & (int)PropertyUsageFlags.ClassIsEnum) != 0;


                        // 实际上，可以获取枚举的名字，但只有用户定义的枚举继承了部分字典的API，可以使用Expression类来获取枚举的名字，
                        // 而内置的枚举不会继承这些API，所以无法根据值获取内置枚举的名字。
                        // 所以为了统一，就都直接显示枚举常量值,
                        // 上面的注释划掉，我找到了获取Godot内置枚举类型枚举名的方法了
                        // 可以通过ShowGDScriptEnumName属性来控制是否显示GDScript枚举名
                        // 如果解析失败则重新显示为枚举值
                        if (isEnum)
                        {
                            var displayName = variant.ToString();
                            // 如果是枚举类型，class_name会是 脚本资源路径.枚举名 / 类名.枚举名
                            var enumName = className;

                            if (ShowGDScriptEnumName)
                            {
                                // 先尝试匹配Godot内置的C#枚举类型
                                if (TryMatchGodotEnumType(enumName, out var matchedEnum))
                                {
                                    var enumValueName = Enum.GetName(matchedEnum!, variant.AsInt64());
                                    displayName = $"{enumValueName}({variant})";
                                }
                                else
                                {
                                    // 用户自定义枚举，利用Expression动态调用GDScript的find_key()
                                    var expObj = gdObject;
                                    var @enum = enumName.Replace($"{script.ResourcePath}.", "");
                                    var names = enumName.Split('.');

                                    // 路径替换失败的情况，即当前枚举来自于单例或其他脚本的实例对象
                                    if (@enum.StartsWith("res://"))
                                    {
                                        var gdIndex = enumName.LastIndexOf(".gd", StringComparison.Ordinal);
                                        if (gdIndex != -1)
                                        {
                                            var scriptPath = enumName.Substring(0, gdIndex + 3); // 包含“.gd”
                                            @enum = enumName.Substring(gdIndex + 4); // 之后的才是枚举名（跳过“.gd.”）

                                            // 由于无法区分单例或其他脚本实例对象，这里统一创建实例，作为表达式的上下文
                                            expObj = GD.Load<GDScript>(scriptPath).New().AsGodotObject();

                                        }

                                    }
                                    // 检查是否是 "全局类.枚举名" 形式
                                    else if (names.Length == 2)
                                    {
                                        var globalClass = names[0];

                                        // 获取全局类信息                                        
                                        var globalClasses = ProjectSettings.GetGlobalClassList();
                                        var classInfo = globalClasses // 找到对应的那个字典
                                            .FirstOrDefault(dict => dict.TryGetValue("class", out var val) && val.ToString() == globalClass);

                                        // 获取这个全局类的脚本路径
                                        if (classInfo != null && classInfo.TryGetValue("path", out var pathValue))
                                        {
                                            var classPath = pathValue.ToString();
                                            if (!string.IsNullOrEmpty(classPath))
                                            {
                                                // Expression类无法访问全局类，这里创建该全局类的对象，为表达式提供上下文
                                                expObj = GD.Load<GDScript>(classPath).New().AsGodotObject();
                                                @enum = names[1]; // names[1]是枚举名
                                            }
                                        }
                                    }
                                    var code = $"{@enum}.find_key({variant})";
                                    var exp = new Expression();

                                    var error = exp.Parse(code, ["variant"]);

                                    if (error != Error.Ok)
                                    {
                                        GD.PushError($"[EnumExecuteError] {exp.GetErrorText()}");
                                    }
                                    else
                                    {
                                        var result = exp.Execute([variant], expObj);
                                        if (!exp.HasExecuteFailed())
                                        {
                                            displayName = $"{result}({variant})";
                                        }
                                        else
                                        {
                                            GD.PushError($"[EnumExecuteError] {exp.GetErrorText()}");
                                        }
                                    }
                                }
                            }

                            var enumNode = _tree.CreateItem(node);
                            SetDefaultColor(enumNode);
                            enumNode.SetText(0, name);
                            enumNode.SetText(1, $"Variant(Int)\nEnum({enumName})");
                            enumNode.SetText(2, displayName);

                            continue;
                        }

                        FillObjectTree(node, variant, name, visited, gdscriptType);
                    }
                }
            }
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
            node.SetText(2, string.IsNullOrEmpty(obj.ToString()) ? "[Empty]" : obj.ToString());
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

    private bool IsUsingGDScript(object obj)
    {
        if (obj is not GodotObject godotObject)
            return false;

        Variant script = godotObject.GetScript();

        return script.VariantType == Variant.Type.Object && script.As<GodotObject>() is GDScript;
    }

    private bool TryMatchGodotEnumType(string gdEnumType, out Type? matchedEnum)
    {
        matchedEnum = null;
        var parts = gdEnumType.Split('.');  // Window.Mode => ["Window", "Mode"]
        if (parts.Length == 1)
        {
            // 只有枚举名, 例如 Error，Side
            matchedEnum = _enumTypes.FirstOrDefault(t => t.Name == parts[0] || t.Name == parts[0] + "Enum");
        }
        else if (parts.Length == 2)
        {
            var className = parts[0]; // Window
            var enumName = parts[1];  // Mode or ModeEnum

            // C# enum FullName : Godot.Window+ModeEnum
            // C# enum Name : ModeEnum
            // C# enum replacement : Godot.Window.ModeEnum
            // gdEnumType : Window.Mode or Window.ModeEnum
            matchedEnum = _enumTypes.FirstOrDefault(t =>
                (t.Name == enumName || t.Name == enumName + "Enum") &&
                t.FullName!.Replace('+', '.').Contains("." + className + "."));
        }
        return matchedEnum != null;
    }

    // 递归查找作为内部类的GDScript实例的路径
    private string? FindNestedScriptPath(GDScript rootScript, GodotObject targetScript)
    {
        string? Recursive(Godot.Collections.Dictionary map, string prefix)
        {
            foreach (var kv in map)
            {
                var key = kv.Key.As<string>();
                var value = kv.Value;

                if (value.VariantType == Variant.Type.Object && value.Obj is GDScript subScript)
                {
                    if (subScript == targetScript)
                        return $"{prefix}{key}";

                    var subMap = subScript.GetScriptConstantMap();
                    var found = Recursive(subMap, $"{prefix}{key}.");
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        var topMap = rootScript.GetScriptConstantMap();
        return Recursive(topMap, $"{rootScript.ResourcePath}.");
    }
    
    private string GetGDScriptClass(GodotObject gdObject, GDScript? root = null)
    {
        if (gdObject.GetScript().Obj is not GDScript gdScript)
            return string.Empty;
        if (gdScript == null)
            return string.Empty;

        if (!gdScript.GetGlobalName().IsEmpty)
            return $"(GlobalClass){gdScript.GetGlobalName()}";

        if (!string.IsNullOrEmpty(gdScript.ResourcePath))
        {
            return gdScript.ResourcePath;
        }
        else if (root != null)
        {            
            var nestedPath = FindNestedScriptPath(root, gdScript);
            if (nestedPath != null)
                return nestedPath;
        }

        return string.Empty;
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
