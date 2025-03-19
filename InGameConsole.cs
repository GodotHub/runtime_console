#if TOOLS
using Godot;

[Tool]
public partial class InGameConsole : EditorPlugin
{
	public override void _EnterTree()
	{
		AddAutoloadSingleton("ConsoleCommands", "res://addons/InGameConsole/ConsoleCommands.cs");
		AddAutoloadSingleton("Console", "res://addons/InGameConsole/Console.tscn");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton("ConsoleCommands");
		RemoveAutoloadSingleton("Console");
	}
}
#endif
