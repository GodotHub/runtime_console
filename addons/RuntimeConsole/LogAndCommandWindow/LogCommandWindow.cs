using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class LogCommandWindow : Window
{
    private enum LogType
    {
        Info,
        Warning,
        Error
    }
    private record class Log(string Message, LogType LogType);
    private readonly Dictionary<LogType, bool> _logTypeVisible = new()
    {
        [LogType.Info] = true,
        [LogType.Warning] = true,
        [LogType.Error] = true
    };

	private LineEdit _consoleInput;
	private RichTextLabel _consoleOutput;
	private VBoxContainer _ioContainer;
	private Button _infoButton;
	private Button _warningButton;
	private Button _errorButton;
	private readonly List<string> _historyCommand = [];
	private readonly List<Log> _logs = [];
	private int _historyIndex = -1;

	private CommandParser _parser = new(Console.GameConsole.GetConfig().ParameterParserPath, Console.GameConsole.GetConfig().CommandPath);

	public override void _Ready()
	{
		_consoleInput = GetNode<LineEdit>("%Input");
		_consoleOutput = GetNode<RichTextLabel>("%Output");
		_ioContainer = GetNode<VBoxContainer>("%IoContainer");
		_infoButton = GetNode<Button>("%Info");
		_warningButton = GetNode<Button>("%Warning");
		_errorButton = GetNode<Button>("%Error");

		_infoButton.Pressed += () =>
		{
			ToggleLogs(LogType.Info);
		};

		_warningButton.Pressed += () =>
		{
			ToggleLogs(LogType.Warning);
		};

		_errorButton.Pressed += () =>
		{
			ToggleLogs(LogType.Error);
		};

		var theme = ThemeDB.GetProjectTheme() ?? ThemeDB.GetDefaultTheme();

		var pressedBox = theme.GetStylebox("pressed", "Button").Duplicate(true) as StyleBoxFlat;
		var focusBox = theme.GetStylebox("focus", "Button").Duplicate(true) as StyleBoxFlat;

		StyleBoxFlat normal = focusBox.Duplicate(true) as StyleBoxFlat;
		normal.BgColor = pressedBox.BgColor;
		normal.DrawCenter = true;

		var buttons = new[] { _infoButton, _warningButton, _errorButton };
		foreach (var btn in buttons)
		{
			btn.AddThemeStyleboxOverride("normal", normal);
			btn.AddThemeStyleboxOverride("hover", normal);
			btn.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
		}


		_consoleInput.TextSubmitted += InputCommand;
		CloseRequested += Hide;

		Size = (Vector2I)GetTree().Root.GetViewport().GetVisibleRect().Size / 2;
	}

	public override void _Process(double delta)
	{
		if (_ioContainer.Visible)
			ShowHistoryCommand();
	}

	/// <summary>
	/// 打印一条日志到控制台
	/// </summary>
	/// <param name="message">消息</param>
	public void Print(params object[] message)
	{
		string formattedMessage = string.Concat($"[{DateTime.Now}][INFO]: ", string.Concat(message), "\n");
		if (_logTypeVisible[LogType.Info])
			_consoleOutput.AppendText(formattedMessage);
		_logs.Add(new(formattedMessage, LogType.Info));
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
		if (_logTypeVisible[LogType.Error])
			_consoleOutput.AppendText(formattedMessage);
		_logs.Add(new(formattedMessage, LogType.Error));
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
		if (_logTypeVisible[LogType.Warning])
			_consoleOutput.AppendText(formattedMessage);
		_logs.Add(new(formattedMessage, LogType.Warning));
	}

	/// <summary>
	/// 打印一条不包含时间戳的日志到控制台
	/// </summary>
	/// <param name="message">消息</param>
	public void PrintRaw(params object[] message)
	{
		var msg = string.Concat(message) + "\n";
		if (_logTypeVisible[LogType.Info])
			_consoleOutput.AppendText(msg);
		_logs.Add(new(msg, LogType.Info));
	}

	/// <summary>
	/// 打印一条不包含时间戳的错误日志到控制台
	/// </summary>
	/// <param name="message">消息</param>
	public void PrintRawError(params object[] message)
	{
		var msg = $"[color=red]{string.Concat(message)}[/color]\n";
		if (_logTypeVisible[LogType.Error])
			_consoleOutput.AppendText(msg);
		_logs.Add(new(msg, LogType.Error));
	}

	// 执行命令
	private void InputCommand(string input)
	{
		if (string.IsNullOrEmpty(input))
			return;
		_historyCommand.Add(input);
		_historyIndex = -1;

		var result = _parser.Parse(input);

		if (result != CommandParser.ParseResult.OK)
		{
			PrintRawError(CommandParser.GetParseMessage(result));
		}

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

	private void ToggleLogs(LogType logType)
	{
		_logTypeVisible[logType] = !_logTypeVisible[logType];		

		var visibleLogs = _logs.Where(log => _logTypeVisible[log.LogType]);
		var builder = new StringBuilder();
		foreach (var log in visibleLogs)
			builder.Append(log.Message);

		_consoleOutput.ParseBbcode(builder.ToString());
	}
}
