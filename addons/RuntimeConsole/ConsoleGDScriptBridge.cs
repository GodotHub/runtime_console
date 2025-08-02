using Godot;
namespace RuntimeConsole;

// Console GDScript Bridge
// 向GDScript脚本提供该类的API，使用GDScript命名规则
partial class Console : Node
{
    Window get_console_window(string key)
        => GetConsoleWindow<Window>(key);
}