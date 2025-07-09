# RuntimeConsole 插件（Godot 游戏内控制台）

**[简体中文](README.md) | [English](README_en.md)**

<img src="RuntimeConsoleIcon.png" width="15%">

## 概述
RuntimeConsole 是一个适用于 Godot .NET 4.4+ 的运行时控制台插件，允许开发者在游戏运行中执行命令、查看日志，并通过对象检查器实时调试场景中的节点和数据结构，为开发与测试带来极大便利

## 功能

运行时查看日志

运行时查看对象属性

运行时执行命令

自由扩展控制台功能窗口

## 路线图

- [x] 自由拓展控制台功能窗口，支持用户添加自定义功能窗口

- [x] 更改日志窗口，支持日志类别筛选

- [ ] 更改日志窗口命令功能自定义方式

- [ ] 更改对象检查器窗口UI，支持运行时编辑属性，调用方法

- [ ] 添加运行时脚本编辑器，支持运行时添加或更改对象的脚本(仅限GDScript)

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

## 添加自定义控制台窗口

1. 新建一个场景，并为该场景附加脚本，该场景脚本必须继承自`Window`，场景根节点必须是`Window`，支持使用`GDScript`编写脚本

2. 实现功能

3. 在插件的`Console Windows`配置界面，点击`Add Window`按钮，填写窗口的键，并选择场景，然后启用

4. 点击`Save`按钮，保存配置

5. 重新加载当前项目

## 注意事项

- 自定义命令机制将在未来版本中优化升级

## 许可证

[`MIT`](https://mit-license.org/) License