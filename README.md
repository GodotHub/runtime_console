# RuntimeConsole 插件（Godot 游戏内控制台）

**[简体中文](README.md) | [English](README_en.md)**

<img src="RuntimeConsoleIcon.png" width="15%">

## 概述
RuntimeConsole 是一个适用于 Godot .NET 4.4+ 的运行时控制台插件，允许开发者在游戏运行中执行命令、查看日志，并通过对象检查器实时调试场景中的节点和数据结构，为开发与测试带来极大便利

## 功能

- 使用 `~` 键打开/关闭控制台。

- Object Inspector（对象检查器）

    * GDScript用户脚本属性显示支持
    
    * 一键显示游戏中的所有节点及其公共实例字段/属性。
    
    * 支持递归对象结构展示，包含字段、属性、列表等复合数据类型。
    
    * 支持搜索关键字并高亮匹配项，轻松定位目标对象。
    
    * 支持自定义显示行为：
    
        * [`[Inspectable]`](/ObjectInspectorWindow/ObjectInspectorWindow.cs/#L340) 自定义字段名\显示该非公共、静态成员。

        * [`[InspectableObject]`](/ObjectInspectorWindow/ObjectInspectorWindow.cs/#L351) 包括非公共、静态成员。

        * [`[HiddenInInspector]`](/ObjectInspectorWindow/ObjectInspectorWindow.cs/#L361) 隐藏特定字段。
        
        * [`[HideInObjectTree]`](/ObjectInspectorWindow/ObjectInspectorWindow.cs/#L366) 从检查器中排除。

- Log & Command Console（日志命令控制台）

    * 实时查看运行时日志（Info / Warning / Error）

    * 执行调试命令

## 环境要求

- [Godot .NET 4.4+](https://godotengine.org/download/windows/)

## 安装

1. 下载`Release`版本并解压到项目中。

2. 在 `Project Settings > Plugins` 中启用插件。


## 添加自定义命令

**未来版本可能更改以下添加自定义命令的方法**

要添加自定义命令，可修改 `ConsoleCommands.cs` 创建新的方法。
每个命令方法：
- 必须接收 `Godot.Collections.Array` 类型的参数。
- 需要自行处理异常。

示例：
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

详见[`ConsoleCommands.cs`](/ConsoleCommands.cs)

## 注意事项

- 自定义命令机制将在未来版本中优化升级

## 许可证

[`MIT`](https://mit-license.org/) License