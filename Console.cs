using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class Console : Node
{
    /// <summary>
    /// 检查器设置
    /// </summary>
    /// <param name="ShowGDScriptObjectProperties">        
    /// 是否展示 GDScript 中枚举属性的名称（如 "Error.OK"），否则仅显示枚举的数值（如 0）。
    /// <br/>
    /// 启用后将遍历 GodotSharp 中所有枚举类型以匹配内置枚举名，
    /// 对性能影响极小，但在极端情况下可能稍微增加初始化时间。        
    /// </param>
    /// <param name="ShowGDScriptEnumName">
    /// 是否在检查器中显示GDScript对象的属性
    /// </param>
    public record class InspectorSetting(bool ShowGDScriptObjectProperties = false, bool ShowGDScriptEnumName = true);

    /// <summary>
    /// 运行时控制台的实例
    /// </summary>
    public static Console GameConsole { get; private set; }
    Button _openConsoleButton;
    HBoxContainer _menu;
    CanvasLayer _canvasLayer;
    readonly Dictionary<string, Window> _windows = []; // 窗口名称 -> 窗口

    public override void _EnterTree()
    {
        GameConsole = this;
        //  添加输入映射
        if (!InputMap.HasAction("switch_console_display"))
        {
            InputMap.AddAction("switch_console_display");
            InputMap.ActionAddEvent("switch_console_display", new InputEventKey()
            {
                Keycode = Key.Quoteleft, // ~键
            });
        }
        // 作为全局加载，设置到最前一层
        GetTree().Root.CallDeferred(Node.MethodName.MoveChild, this, -1);

        _openConsoleButton = GetNode<Button>("%OpenConsoleButton");
        _menu = GetNode<HBoxContainer>("%Menu");
        _canvasLayer = GetNode<CanvasLayer>("%CanvasLayer");

        _openConsoleButton.Pressed += () =>
        {
            foreach (var window in _windows.Values)
            {
                window.Visible = false;
            }
            _menu.Visible = !_menu.Visible;            
        };

        CreateWindows();

    }

    public override void _ExitTree()
    {
        if (InputMap.HasAction("switch_console_display"))
        {
            InputMap.EraseAction("switch_console_display");
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("switch_console_display"))
        {
            HideWindows();
        }
    }

    /// <summary>
    /// 更改检查器设置
    /// </summary>
    /// <param name="setting">检查器设置实例</param>
    public void ChangeInspectorSettings(InspectorSetting setting)
    {
        var inspectorWindow = GetConsoleWindow<ObjectInspectorWindow>("Object Inspector");
        if (inspectorWindow == null)
        {
            GD.PrintErr("[RuntimeConsole]: Object Inspector window does not enabled");
            return;
        }
        inspectorWindow.ShowGDScriptObjectProperties = setting.ShowGDScriptObjectProperties;
        inspectorWindow.ShowGDScriptEnumName = setting.ShowGDScriptEnumName;
    }

    /// <summary>
    /// 使用配置中的键获取窗口实例
    /// </summary>
    /// <typeparam name="T">控制台窗口的类型</typeparam>
    /// <param name="key">配置中设定的窗口的键</param>
    /// <returns>控制台窗口实例，失败时返回null</returns>    
    public T GetConsoleWindow<T>(string key) where T : Window
    {
        if (!_windows.TryGetValue(key, out var window))
        {
            return null;
        }

        return (T)window;
    }


    private void CreateWindows()
    {
        // 默认配置
        var defaultWindowSettings = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>
        {
            new(){
                { "Log and Command", "res://addons/RuntimeConsole/LogAndCommandWindow/LogCommand.tscn"},
                { "enabled", true}
            },
            new(){
                { "Object Inspector", "res://addons/RuntimeConsole/ObjectInspectorWindow/ObjectInspector.tscn"},
                { "enabled", true}
            },
        };

        // 获取配置，获取失败使用默认配置
        var windowSettings = ProjectSettings.GetSetting("runtime_console/window_settings", defaultWindowSettings)
            .AsGodotArray<Godot.Collections.Dictionary<string, Variant>>();

        // GD.Print(windowSettings);

        foreach (var setting in windowSettings)
        {
            // 排除enabled字段
            var keyValue = setting.FirstOrDefault(kvp => kvp.Key != "enabled");
            if (keyValue.Equals(default(KeyValuePair<string, Variant>)))
                continue;

            var key = keyValue.Key;
            var path = keyValue.Value.AsString();

            // 过滤未启用的窗口            
            if (!setting.TryGetValue("enabled", out var enabled) || !enabled.AsBool())
            {
                GD.Print($"[RuntimeConsole] Window " + key + " is disabled");
                continue;
            }


            var button = new Button()
            {
                Text = key,
                Name = key,
            };

            // 加载窗口
            var prefab = ResourceLoader.Load<PackedScene>(path);
            var window = prefab.Instantiate();

            // 窗口类型不正确
            if (window is not Window validWindow)
            {
                GD.PrintErr("[RuntimeConsole] Failed to load console window: " + path);
                continue;
            }

            validWindow.Name = key;
            button.AddChild(validWindow);
            button.Pressed += () => validWindow.Visible = !validWindow.Visible;

            _windows.Add(key, validWindow);
            _menu.AddChild(button);
        }
    }

    private void HideWindows()
    {
        foreach (var window in _windows.Values)
        {
            window.Visible = false;
        }
        _menu.Visible = !_canvasLayer.Visible;            
        _canvasLayer.Visible = !_canvasLayer.Visible;
    }

}