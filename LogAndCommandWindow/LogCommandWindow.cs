using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class LogCommandWindow : Window
{
    private LineEdit _consoleInput;
    private RichTextLabel _consoleOutput;
    private VBoxContainer _IoContainer;
    private readonly List<string> _historyCommand = [];
    private int _historyIndex = -1;

    public override void _EnterTree()
    {
        _consoleInput = GetNode<LineEdit>("%Input");
        _consoleOutput = GetNode<RichTextLabel>("%Output");
        _IoContainer = GetNode<VBoxContainer>("%IoContainer");
        _consoleInput.TextSubmitted += InputCommand;
        CloseRequested += Hide;        
    }

    public override void _Process(double delta)
    {
        if (_IoContainer.Visible)
            ShowHistoryCommand();
    }

    /// <summary>
    /// 打印一条日志到控制台
    /// </summary>
    /// <param name="message">消息</param>
    public void Print(params object[] message)
    {
        string formattedMessage = string.Concat($"[{DateTime.Now}][INFO]: ", string.Concat(message), "\n");
        _consoleOutput.AppendText(formattedMessage);
    }

    /// <summary>
    /// 打印一条错误日志到控制台
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintError(params object[] message)
    {
        string formattedMessage = string.Concat(
            $"[color=red][{DateTime.Now}][ERROR]: ",
            string.Concat(message),
            "[/color]\n"
        );
        _consoleOutput.AppendText(formattedMessage);
    }
    
    /// <summary>
    /// 打印一条警告日志到控制台
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintWarning(params object[] message)
    {
        string formattedMessage = string.Concat(
            $"[color=yellow][{DateTime.Now}][WARNING]: ",
            string.Concat(message),
            "[/color]\n"
        );
        _consoleOutput.AppendText(formattedMessage);
    }

    /// <summary>
    /// 打印一条不包含时间戳的日志到控制台
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintNoFormattedMessage(params object[] message)
    {
        _consoleOutput.AppendText(string.Concat(message) + "\n");
    }

    /// <summary>
    /// 打印一条不包含时间戳的错误日志到控制台
    /// </summary>
    /// <param name="message">消息</param>
    public void PrintNoFormattedErrorMessage(params object[] message)
    {
        _consoleOutput.AppendText($"[color=red]{string.Concat(message)}[/color]\n");
    }

    // 执行命令（TODO:后续可能会更改实现）
    private void InputCommand(string input)
    {
        string[] commandAndArgs = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (commandAndArgs.Length == 0) return;

        // 分割命令和参数
        string command = commandAndArgs[0];
        _historyCommand.Add(input);
        _historyIndex = -1;

        if (!ConsoleCommands.Commands.HasMethod(command))
        {
            PrintNoFormattedErrorMessage($"Invalid command : {command}");
            _consoleInput.Text = "";
            return;
        }

        Godot.Collections.Array validArgs = new(commandAndArgs
            .Skip(1)
            .Select(Variant.CreateFrom)
            .ToArray());

        // 在单例上调用指定命令
        ConsoleCommands.Commands.Call(command, validArgs);
        _consoleInput.Text = "";
    }

    // 显示历史命令
    private void ShowHistoryCommand()
    {
        if (_historyCommand.Count == 0) return;

        if (Input.IsActionJustPressed("ui_up"))
        {
            NavigateHistory(-1);
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            NavigateHistory(1);
        }
    }

    private void NavigateHistory(int direction)
    {
        if (direction == -1) // 向上导航
        {
            _historyIndex--;
            if (_historyIndex < 0)
                _historyIndex = _historyCommand.Count - 1;
        }
        else if (direction == 1) // 向下导航
        {
            _historyIndex = (_historyIndex + 1) % _historyCommand.Count;            
        }

        SetConsoleInputText(_historyCommand[_historyIndex]);
    }

    // 设置控制台输入框的文本
    private void SetConsoleInputText(string text)
    {
        _consoleInput.Text = text;
        _consoleInput.GrabFocus();
        _consoleInput.CaretColumn = _consoleInput.Text.Length;
    }
}
