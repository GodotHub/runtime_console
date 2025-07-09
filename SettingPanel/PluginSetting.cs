#if TOOLS
using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class PluginSetting : Control
{
	public record class ConsoleWindowPathSetting
	{
		// 窗口的键，标题
		public string Key { get; }
		// 窗口的预制件路径
		public string Path { get; }
		// 是否启用该窗口
		public bool Enabled { get; }
		public ConsoleWindowPathSetting(string key, string path, bool enabled)
		{
			Key = key;
			Path = path;
			Enabled = enabled;
		}
	}
	PackedScene _windowSelectBox = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/SettingPanel/UIComponent/WindowSelectBox.tscn");
	// 默认的窗口配置
	readonly List<ConsoleWindowPathSetting> _defaultWindowPaths =
	[
		new ConsoleWindowPathSetting("Log and Command","res://addons/RuntimeConsole/LogAndCommandWindow/LogCommand.tscn", true),
		new ConsoleWindowPathSetting("Object Inspector", "res://addons/RuntimeConsole/ObjectInspectorWindow/ObjectInspectorWindow.tscn", true),
	];
	[Export] VBoxContainer _container;
	[Export] Button _addButton;
	[Export] Button _saveButton;
	public override void _Ready()
	{
		LoadSettings(); // 加载配置

		_addButton.Pressed += () =>
		{
			var windowSelectBox = _windowSelectBox.Instantiate<WindowSelectBox>();
			_container.AddChild(windowSelectBox);
		};

		_saveButton.Pressed += SaveSettings;
	}

	public override void _Process(double delta)
	{

	}

	const string ConfigFilePath = "user://runtime_console_settings.json";

	void LoadSettings()
	{
		var config = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>();
		// 配置文件存在，从文件读取配置
		if (FileAccess.FileExists(ConfigFilePath))
		{
			using var file = FileAccess.Open(ConfigFilePath, FileAccess.ModeFlags.Read);
			var json = file.GetAsText();

			var jsonData = Json.ParseString(json);
			if (jsonData.VariantType != Variant.Type.Nil)
			{
				var data = jsonData.AsGodotArray<Godot.Collections.Dictionary<string, Variant>>();

				foreach (var setting in data)
				{
					// [{"windows":"window/path","enbaled":true}]
					// 排除掉enabled字段
					var keyValue = setting.FirstOrDefault(kv => kv.Key != "enabled");
					if (keyValue.Equals(default(KeyValuePair<string, Variant>)))
						continue;
					


					// 构造窗口选择框
					var windowSelectBox = _windowSelectBox.Instantiate<WindowSelectBox>();

					windowSelectBox.Key = keyValue.Key;
					windowSelectBox.Path = keyValue.Value.AsString();
					windowSelectBox.Enabled = setting["enabled"].AsBool();

					// 创建项目配置，用于在运行时访问
					config.Add(new()
					{
						{keyValue.Key, keyValue.Value},
						{"enabled", true}
					});

					// 添加到容器
					_container.AddChild(windowSelectBox);
				}
				// 添加项目配置
				ProjectSettings.SetSetting("runtime_console/window_settings", config);
				return;
			}
		}


		// 配置文件不存在，加载默认配置
		foreach (var setting in _defaultWindowPaths)
		{
			var windowSelectBox = _windowSelectBox.Instantiate<WindowSelectBox>();
			windowSelectBox.Key = setting.Key;
			windowSelectBox.Path = setting.Path;
			windowSelectBox.Enabled = setting.Enabled;

			config.Add(new()
			{
				{setting.Key, setting.Path},
				{"enabled", setting.Enabled}
			});
			_container.AddChild(windowSelectBox);
		}
		ProjectSettings.SetSetting("runtime_console/window_settings", config);
		
	}

	void SaveSettings()
	{
		var windowSettings = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>();
		foreach (var windowSelectBox in _container.GetChildren())
		{
			// 排除不合法子项
			if (windowSelectBox is not WindowSelectBox box)
				continue;

			// [{"windows":"window/path","enbaled":true}]
			var windowSelectBoxDict = new Godot.Collections.Dictionary<string, Variant>
			{
				{box.Key, box.Path},
				{"enabled", box.Enabled}
			};

			windowSettings.Add(windowSelectBoxDict);
		}

		// 保存配置文件
		using var file = FileAccess.Open(ConfigFilePath, FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(windowSettings, "\t"));

		// 保存到项目设置，以供运行时访问
		ProjectSettings.SetSetting("runtime_console/window_settings", windowSettings);
	}
	
}
#endif