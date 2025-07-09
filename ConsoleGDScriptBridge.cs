using Godot;
namespace RuntimeConsole;

// Console GDScript Bridge
// 向GDScript脚本提供该类的API，使用GDScript命名规则
partial class Console : Node
{
    Window get_console_window(string key)
        => GetConsoleWindow<Window>(key);
    void change_inspector_settings(bool show_gdscript_enum_name, bool show_gdscript_object_properties)
        => ChangeInspectorSettings(
            new InspectorSetting()
            {
                ShowGDScriptObjectProperties = show_gdscript_object_properties,
                ShowGDScriptEnumName = show_gdscript_enum_name
            }
        );
}