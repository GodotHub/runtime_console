using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class Console : Node
{

    /// <summary>
    /// 运行时控制台的实例
    /// </summary>
    public static Console GameConsole { get; private set; }    
    Button _openConsoleButton;
    HBoxContainer _menu;
    CanvasLayer _canvasLayer;
    readonly Dictionary<string, Window> _windows = []; // 窗口名称 -> 窗口

    readonly PluginSetting _callback = new()
    {
        WindowSettings = [
            new(
                key:"Log and Command",
                window: ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/LogAndCommandWindow/LogCommand.tscn"),
                enabled: true
            ),

            new(
                key:"Object Tree",
                window: ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectTreeWindow/ObjectTreeWindow.tscn"),
                enabled: true
            ),

            new(
                key:"Object Inspector",
                window: ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/ObjectInspectorWindow.tscn"),
                enabled: true
            ),
        ],
        CommandPath = "res://addons/RuntimeConsole/LogAndCommandWindow/CommandComponent/Commands",
        ParameterParserPath = "res://addons/RuntimeConsole/LogAndCommandWindow/CommandComponent/ParameterParser",
        EditorIconsTheme = ResourceLoader.Load<Theme>("res://addons/RuntimeConsole/ObjectTreeWindow/editor_icons.tres")		    
    };
    
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

    /// <summary>
    /// 获取插件配置
    /// </summary>
    /// <returns>插件配置，读取失败时返回默认配置</returns>
    public PluginSetting GetConfig()
    {
        if (!ResourceLoader.Exists("res://addons/RuntimeConsole/Config/config.tres"))
        {
            GD.PrintErr("[RuntimeConsole]: Config file not found, using default settings.");
            return _callback;
        }

        var pluginSetting = ResourceLoader.Load<PluginSetting>("res://addons/RuntimeConsole/Config/config.tres");

        return pluginSetting;
    }

    private void CreateWindows()
    {
        var config = GetConfig();
        foreach (var windowSetting in config.WindowSettings)
        {
            if (!windowSetting.Enabled)
            {
                continue;
            }

            if (windowSetting.Window == null)
            {
                GD.PrintErr($"[RuntimeConsole]: Window '{windowSetting.Key}' has no scene assigned.");
                continue;
            }

            if (_windows.ContainsKey(windowSetting.Key))
            {
                GD.PrintErr($"[RuntimeConsole]: Window '{windowSetting.Key}' already exists.");
                continue;
            }

            var window = windowSetting.Window.Instantiate<Window>();
            window.Name = windowSetting.Key;
            window.Visible = false;
            var button = new Button()
            {
                Text = windowSetting.Key,
                Name = windowSetting.Key,
            };
            button.Pressed += () => window.Visible = !window.Visible;
            button.AddChild(window);
            _menu.AddChild(button);
            _windows[windowSetting.Key] = window;
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