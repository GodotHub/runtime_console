using Godot;

namespace RuntimeConsole;

// LogCommandWindowGDScriptBridge
// 向GDScript脚本提供该类的API，使用GDScript命名规则
// 注册为全局类以供GDScript使用类型检查，你不应该从编辑器创建该类的实例
[GlobalClass]
partial class LogCommandWindow : Window
{
    void print(Variant msg)
        => Print(msg.ToString());

    void print_error(Variant msg)
        => PrintError(msg.ToString());

    void print_warning(Variant msg)
        => PrintWarning(msg.ToString());

    void print_raw(Variant msg)
        => PrintRaw(msg.ToString());

    void print_raw_error(Variant msg)
        => PrintRawError(msg.ToString());
}