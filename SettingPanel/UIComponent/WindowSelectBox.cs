#if TOOLS
using Godot;

[Tool]
public partial class WindowSelectBox : HBoxContainer
{
	public string Key
	{
		get
		{
			_key ??= GetNode<LineEdit>("%Key");
			return _key.Text;
		}
		set
		{
			_key ??= GetNode<LineEdit>("%Key");
			_key.Text = value;
		}
	}

	public string Path
	{
		get
		{
			_path ??= GetNode<LineEdit>("%Path");
			return _path.Text;
		}
		set
		{
			_path ??= GetNode<LineEdit>("%Path");
			_path.Text = value;
		}
	}

	public bool Enabled
	{
		get
		{
			_checkButton ??= GetNode<CheckButton>("%CheckButton");
			return _checkButton.ButtonPressed;
		}
		set
		{
			_checkButton ??= GetNode<CheckButton>("%CheckButton");
			_checkButton.ButtonPressed = value;			
			OnEnableToggled(value);
		}
	}

	[Export] LineEdit _key;
	[Export] LineEdit _path;
	[Export] Button _pathSelectButton;
	[Export] CheckButton _checkButton;
	[Export] Button _removeButton;

	public override void _Ready()
	{
		_pathSelectButton ??= GetNode<Button>("PathSelectButton");
		_removeButton ??= GetNode<Button>("RemoveButton");

		_checkButton.Toggled += OnEnableToggled;
		_removeButton.Pressed += QueueFree;

		// 打开文件选择窗口
		_pathSelectButton.Pressed += () => DisplayServer.FileDialogShow(
			title: "Select Console Window Extension Scene File",
			currentDirectory: "res://",
			fileName: "",
			showHidden: false,
			mode: DisplayServer.FileDialogMode.OpenFile,
			filters: ["*.tscn"],
			callback: Callable.From<bool, string[], long>(OnFileSelected));


		_pathSelectButton.Icon = EditorInterface.Singleton.GetEditorTheme().GetIcon("DirAccess", "EditorIcons");
		_removeButton.Icon = EditorInterface.Singleton.GetEditorTheme().GetIcon("Close", "EditorIcons");

	}

	void OnEnableToggled(bool enabled)
	{		
		// 禁止编辑，因为已经启用窗口
		_pathSelectButton?.SetDisabled(enabled);
		_path.Editable = !enabled;
		_key.Editable = !enabled;
	}

	void OnFileSelected(bool status, string[] selectedPaths, long selectedFilterIndex)
	{
		if (!status) // status为false，应该是取消选择，或者选择失败什么的
			return;

		if (selectedPaths.Length < 1) // 一个文件也没选
			return;

		string path = selectedPaths[0]; // 只看第一个文件

		if (!FileAccess.FileExists(path)) // 文件不存在
		{
			GD.PrintErr($"[RuntimeConsole]: Selected file does not exist: {path}");
			return;
		}

		// 不是项目文件
		if (!path.Contains(ProjectSettings.GlobalizePath("res://")))
		{
			GD.PrintErr($"[RuntimeConsole]: Selected file is not in the project directory: {path}");
			return;
		}

		// 把绝对路径转为项目路径
		_path.Text = ProjectSettings.LocalizePath(path);
		
	}
}
#endif
