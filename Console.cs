#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/*
    in-game console
    Pressed "~" to open the console on debug (You can change the shortcut in the Godot editor)
    Pressed "↑/↓" to navigate through the history of command
    Pressed "Enter" to execute the command
*/

public partial class Console : Node
{
    public static Console GameConsole {get; private set;}
    private Button _openConsoleButton;

    private LineEdit _consoleInput;

    private RichTextLabel _consoleOutput;

    private VBoxContainer _IoContainer;

    private readonly List<string> _historyCommand =  [];

    private int _historyIndex = -1;

    public override void _Ready()
    {
        GameConsole = this;
        // As a autoload, set it to the top layer
        GetTree().Root.CallDeferred("move_child", this, -1);

        _openConsoleButton = GetNode<Button>("%OpenConsoleButton");
        _consoleInput = GetNode<LineEdit>("%Input");
        _consoleOutput = GetNode<RichTextLabel>("%Output");
        _IoContainer = GetNode<VBoxContainer>("%IoContainer");

        _openConsoleButton.Pressed += ShowConsole;

        _consoleInput.TextSubmitted += InputCommand;
    }

    public override void _Process(double delta)
    {
        if (_IoContainer.Visible)
            ShowHistoryCommand();
    }

    private void ShowConsole()
        => _IoContainer.Visible = !_IoContainer.Visible;

    /// <summary>
    /// Print a message with thimestamp in the console
    /// </summary>
    /// <param name="message">The message to print</param>
    public void Print(params object[] message)
    {
        string formattedMessage  = string.Concat($"[{DateTime.Now}][INFO]: ",  string.Concat(message), "\n");
        _consoleOutput.AppendText(formattedMessage);
    }

    /// <summary>
    /// Print a error message with thimestamp in the console
    /// </summary>
    /// <param name="message">The error message to print</param>
    public void PrintError(params object[] message)
    {
        string formattedMessage = string.Concat(
            $"[color=red][{DateTime.Now}][ERROR]: ", 
            string.Concat(message), 
            "[/color]\n"
        );
        _consoleOutput.AppendText( formattedMessage);
    }
    
    /// <summary>
    /// Print a warning message with thimestamp in the console
    /// </summary>
    /// <param name="message">The warning message to print</param>
    public void PrintWarning(params object[] message)
    {
        string formattedMessage = string.Concat(
            $"[color=yellow][{DateTime.Now}][WARNING]: ", 
            string.Concat(message), 
            "[/color]\n"
        );
        _consoleOutput.AppendText( formattedMessage);
    }

    /// <summary>
    /// Print a message without formatted(timestamp) in the console
    /// </summary>
    /// <param name="message">The message to print</param>
    public void PrintNoFormattedMessage(string message)
        => _consoleOutput.AppendText(message + "\n");

    /// <summary>
    /// Print a error message without formatted(timestamp) in the console
    /// </summary>
    /// <param name="message">The error message to print</param>
    public void PrintNoFormattedErrorMessage(string message)
        => _consoleOutput.AppendText($"[color=red]{message}[/color]\n");

    /// <summary>
    /// Clear the console
    /// </summary>
    public void Clear()
        => _consoleOutput.Clear();

    private void InputCommand(string input)
    {
        string[] commandAndArgs = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (commandAndArgs.Length == 0) return;

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

        ConsoleCommands.Commands.Call(command, validArgs);
        _consoleInput.Text = "";
    }

#region ShowHistory
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
        if (direction == -1) // Navigate upwards
        {
            _historyIndex = _historyIndex == -1 ? _historyCommand.Count - 1 : Math.Max(_historyIndex - 1, -1);
        }
        else if (direction == 1) // Navigate downward
        {
            _historyIndex = Math.Min(_historyIndex + 1, _historyCommand.Count);
        }

        if (_historyIndex == -1 || _historyIndex >= _historyCommand.Count)
        {
            _consoleInput.Text = "";
            _historyIndex = -1;
        }
        else
        {
            SetConsoleInputText(_historyCommand[_historyIndex]);
        }
    }

    private void SetConsoleInputText(string text)
    {
        _consoleInput.Text = text;
        _consoleInput.GrabFocus();
        _consoleInput.CaretColumn = _consoleInput.Text.Length;
    }

#endregion

}
#endif