# RuntimeConsole Plugin (In-Game Console for Godot)

**[简体中文](README.md) | [English](README_en.md)**

<img src="RuntimeConsoleIcon.png" width="15%">

## Overview
RuntimeConsole is a runtime console plugin for Godot .NET 4.4+ that allows developers to execute commands, view logs, and debug scene nodes and data structures in real-time during gameplay, providing great convenience for development and testing.

## Features

View logs at runtime

Inspect object properties at runtime

Execute commands at runtime

Freely extend console feature windows

## Roadmap

- [x] Freely extend console feature windows, support for adding custom feature windows

- [x] Improve log window, support log category filtering

- [ ] Improve log window command customization

- [ ] Improve object inspector UI, support editing properties and calling methods at runtime

- [ ] Add runtime script editor, support adding or modifying object scripts at runtime (GDScript only)

## Requirements

- [Godot .NET 4.4+](https://godotengine.org/download/windows/)

## Installation

1. Download the `Release` version and extract it into your project.

2. Enable the plugin in `Project Settings > Plugins`.

## Adding Custom Commands

**Note: The method for adding custom commands may change in future versions**

To add custom commands, modify `ConsoleCommands.cs` to create new methods.
Each command method must:
- Accept a parameter of type `Godot.Collections.Array`.
- Handle exceptions manually.

Example:
```csharp
private void Greet(Godot.Collections.Array args)
{
    if (args.Count < 1)
    {
        Console.GameConsole.PrintNoFormattedErrorMessage("Usage: Greet <name>");
        return;
    }
    Console.GameConsole.PrintNoFormattedMessage($"Hello, {args[0]}!");
}
```

See [`ConsoleCommands.cs`](/ConsoleCommands.cs) for details.

## Add Custom Console Window

1. Create a new scene and attach a script to it. The script must inherit from `Window`, and the root node of the scene must be `Window`. GDScript is supported.

2. Implement your desired functionality.

3. In the plugin's `Console Windows` configuration interface, click the `Add Window` button, enter the window key, select the scene, and enable it.

4. Click the `Save` button to save the configuration.

5. Reload the current project.


## Notes

- Custom command mechanism will be optimized in future versions

## License

[`MIT`](https://mit-license.org/) License
