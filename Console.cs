using System.Collections.Generic;
using Godot;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class Console : Node
{
    public record class InspectorSetting
    {
        /// <summary>
        /// 是否展示 GDScript 中枚举属性的名称（如 "Error.OK"），否则仅显示枚举的数值（如 0）。
        /// <br/>
        /// 启用后将遍历 GodotSharp 中所有枚举类型以匹配内置枚举名，
        /// 对性能影响极小，但在极端情况下可能稍微增加初始化时间。
        /// </summary>
        public bool ShowGDScriptObjectProperties { get; init; } = true;
        /// <summary>
        /// 是否在检查器中显示GDScript对象的属性
        /// </summary>
        public bool ShowGDScriptEnumName { get; init; } = false;
    }

    public static Console GameConsole { get; private set; }
    private Button _openConsoleButton;
    private HBoxContainer _menu;
    private CanvasLayer _canvasLayer;
    private LogCommandWindow _logWindow;
    private ObjectInspectorWindow _inspector;
    private readonly Dictionary<string, Button> _buttons = []; // 按钮名称 -> 按钮
    private readonly Dictionary<string, Window> _windows = []; // 窗口名称 -> 窗口

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
        GetTree().Root.CallDeferred("move_child", this, -1);

        _openConsoleButton = GetNode<Button>("%OpenConsoleButton");
        _menu = GetNode<HBoxContainer>("%Menu");
        _canvasLayer = GetNode<CanvasLayer>("%CanvasLayer");

        // 加载所有的窗口
        foreach (var button in _menu.GetChildren())
        {
            if (button is Button b && b.GetChildCount() > 0 && b.GetChild(0) is Window window)
            {
                _buttons.Add(b.Name, b);
                _windows.Add(window.Name, window);

                if (window is LogCommandWindow log)
                {
                    _logWindow = log;
                }
                if (window is ObjectInspectorWindow window1)
                {
                    _inspector = window1;
                }

                b.Pressed += () => window.Visible = !window.Visible;
            }
        }


        _openConsoleButton.Pressed += () => _menu.Visible = !_menu.Visible;

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
            foreach (var window in _windows.Values)
            {
                window.Visible = false;
            }
            _canvasLayer.Visible = !_canvasLayer.Visible;
        }
    }

    /// <summary>
    /// 配置对象检查器
    /// </summary>
    /// <param name="setting">一个检查器设置实例</param>
    public void SettingInspector(InspectorSetting setting)
    {
        _inspector.ShowGDScriptEnumName = setting.ShowGDScriptEnumName;
        _inspector.ShowGDScriptObjectProperties = setting.ShowGDScriptObjectProperties;
    }

    /// <summary>
    /// 获取当前的对象检查器配置
    /// </summary>
    /// <returns>返回一个检查器设置实例</returns>
    public InspectorSetting GetCurrentInspectorSetting()
    {
        return new()
        {
            ShowGDScriptEnumName = _inspector.ShowGDScriptEnumName,
            ShowGDScriptObjectProperties = _inspector.ShowGDScriptObjectProperties
        };
    }

    /// <summary>
    /// 打印一条日志到控制台（对日志窗口方法的快捷访问）
    /// </summary>
    /// <param name="message">消息</param>
    public void Print(params object[] message)
    {
        _logWindow?.Print(message);
    }

    /// <summary>
    /// 打印一条错误日志到控制台（对日志窗口方法的快捷访问）
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintError(params object[] message)
    {
        _logWindow?.PrintError(message);
    }

    /// <summary>
    /// 打印一条警告日志到控制台（对日志窗口方法的快捷访问）
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintWarning(params object[] message)
    {
        _logWindow?.PrintWarning(message);
    }

    /// <summary>
    /// 打印一条不包含时间戳的日志到控制台（对日志窗口方法的快捷访问）
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintNoFormattedMessage(params object[] message)
    {
        _logWindow?.PrintNoFormattedMessage(message);
    }

    /// <summary>
    /// 打印一条不包含时间戳的错误日志到控制台（对日志窗口方法的快捷访问）
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintNoFormattedErrorMessage(params object[] message)
    {
        _logWindow?.PrintNoFormattedErrorMessage(message);
    }

}