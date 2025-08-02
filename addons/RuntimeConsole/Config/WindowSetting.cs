using Godot;

namespace RuntimeConsole;

[GlobalClass]
public partial class WindowSetting : Resource
{
    [Export] public string Key { get; private set; }
    [Export] public PackedScene Window { get; private set; }
    [Export] public bool Enabled { get; private set; }    

    public WindowSetting()
    {
        Key = string.Empty;
        Window = null;
        Enabled = true;
    }

    public WindowSetting(string key, PackedScene window, bool enabled)
    {
        Key = key;
        Window = window;
        Enabled = enabled;
    }
}