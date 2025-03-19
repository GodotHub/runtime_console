#  Godot In-Game Console Plugin

## Overview
This plugin provides an in-game console for debugging and executing commands during runtime in Godot. It allows developers to quickly test commands and retrieve debug information without modifying the game code.

## Features
- Open console with `~` key (configurable in Godot editor).
- Execute commands by typing in the console and pressing `Enter`.
- Navigate through command history using `↑` and `↓` keys.
- Print messages, warnings, and errors in a formatted way.
- Clear console output.
- Easily extendable with custom commands.

## Installation
1. Copy the all files into your Godot project’s `addons/InGameConsole` directory.
2. Enable the plugin in `Project Settings > Plugins`.

## Usage
- Press `~` to open/close the console.
- Type commands and press `Enter` to execute.
- Use `clear` command to clear the console.

### Example Commands
```text
ExampleCommand           # Prints a test message
ExampleCommand2 5        # Calculates the square of 5
ClearConsole             # Clears the console output
```

## Extending the Console
To add custom commands, modify `ConsoleCommands.cs` and create new public methods. 
Each command method:
- Must have a single parameter of type `Godot.Collections.Array`.
- Should handle exceptions internally.

Example:
```csharp
public void Greet(Godot.Collections.Array args)
{
    if (args.Count < 1)
    {
        GameConsole.PrintNoFormattedErrorMessage("Usage: Greet <name>");
        return;
    }
    GameConsole.PrintNoFormattedMessage($"Hello, {args[0]}!");
}
```

### Example of custom commands in ConsoleCommands.cs:

```csharp
    public static void GivePlayerItem(Godot.Collections.Array args)
    {
        try
        {
            uint itemId = args[0].AsUInt32();
            int itemQuantity = args[1].AsInt32();
            if (ItemManager.AddItem(itemId, itemQuantity, true))
                GameConsole.PrintNoFormattedMessage($"{itemQuantity} {itemId} given to player inventory!");
            else
                throw new ArgumentException();
        }
        catch (Exception)
        {
            GameConsole.PrintNoFormattedErrorMessage("Invalid arguments! Need to specify valid item name and quantity!");
        }
    }

    public static void DB(Godot.Collections.Array _)
    {
        GameConsole.PrintNoFormattedMessage("Database follow below:");
        GameConsole.PrintNoFormattedMessage(
                JsonConvert.SerializeObject(
                    new Dictionary<string, object>()
                    {
                        ["ItemDB"] = GameDB.ItemDB.Values.Select(x => x.ToJson()),
                        ["UpgradeDB"] = GameDB.UpgradeDB.Values.Select(x => x.ToJson()),
                        ["ChallengeDB"] = GameDB.ChallengeDB.Values.Select(x => x.ToJson())
                    },
                    Formatting.Indented
                )
            );
    }
```

---

# Godot 游戏内控制台

## 概述
此插件为 Godot 提供了一个游戏内控制台，允许在运行时执行命令和调试，方便开发者快速测试命令并获取调试信息。

## 功能
- 通过 `~` 键打开/关闭控制台（可在 Godot 编辑器中修改）。
- 在控制台输入命令并按 `Enter` 执行。
- 使用 `↑` 和 `↓` 键浏览历史命令。
- 格式化输出普通信息、警告和错误。
- 清空控制台。
- 轻松扩展自定义命令。

## 安装
1. 将所有文件复制到 Godot 项目的 `addons/InGameConsole` 目录。
2. 在 `Project Settings > Plugins` 中启用插件。

## 使用方法
- 按 `~` 键打开/关闭控制台。
- 输入命令后按 `Enter` 执行。
- 使用 `clear` 命令清空控制台。

### 示例命令
```text
ExampleCommand           # 打印测试信息
ExampleCommand2 5        # 计算 5 的平方
ClearConsole             # 清空控制台输出
```

## 扩展控制台
要添加自定义命令，可修改 `ConsoleCommands.cs` 并创建新的 `public` 方法。
每个命令方法：
- 必须接收 `Godot.Collections.Array` 类型的参数。
- 需要自行处理异常。

示例：
```csharp
public void Greet(Godot.Collections.Array args)
{
    if (args.Count < 1)
    {
        GameConsole.PrintNoFormattedErrorMessage("Usage: Greet <name>");
        return;
    }
    GameConsole.PrintNoFormattedMessage($"Hello, {args[0]}!");
}
```

### 在ConsoleCommands.cs自定义命令示例:

```csharp
    public static void GivePlayerItem(Godot.Collections.Array args)
    {
        try
        {
            uint itemId = args[0].AsUInt32();
            int itemQuantity = args[1].AsInt32();
            if (ItemManager.AddItem(itemId, itemQuantity, true))
                GameConsole.PrintNoFormattedMessage($"{itemQuantity} {itemId} given to player inventory!");
            else
                throw new ArgumentException();
        }
        catch (Exception)
        {
            GameConsole.PrintNoFormattedErrorMessage("Invalid arguments! Need to specify valid item name and quantity!");
        }
    }

    public static void DB(Godot.Collections.Array _)
    {
        GameConsole.PrintNoFormattedMessage("Database follow below:");
        GameConsole.PrintNoFormattedMessage(
                JsonConvert.SerializeObject(
                    new Dictionary<string, object>()
                    {
                        ["ItemDB"] = GameDB.ItemDB.Values.Select(x => x.ToJson()),
                        ["UpgradeDB"] = GameDB.UpgradeDB.Values.Select(x => x.ToJson()),
                        ["ChallengeDB"] = GameDB.ChallengeDB.Values.Select(x => x.ToJson())
                    },
                    Formatting.Indented
                )
            );
    }
```