using Godot;
using Godot.Collections;

namespace RuntimeConsole;

[GlobalClass]
public partial class PluginSetting : Resource
{
	[ExportGroup("Window Setting")]
	[Export] public Array<WindowSetting> WindowSettings;

	[ExportGroup("Command Setting")]
	[Export(PropertyHint.Dir)] public string CommandPath = "res://addons/RuntimeConsole/LogAndCommandWindow/CommandComponent/Commands";
	[Export(PropertyHint.Dir)] public string ParameterParserPath = "res://addons/RuntimeConsole/LogAndCommandWindow/CommandComponent/ParameterParser";


	[ExportGroup("ObjectInspector Setting")]
	[Export] public Theme EditorIconsTheme = ResourceLoader.Load<Theme>("res://addons/RuntimeConsole/ObjectTreeWindow/editor_icons.tres");
}
