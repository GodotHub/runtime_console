using Godot;
using System;
using System.Collections.Generic;

namespace RuntimeConsole;

public partial class Clipboard : Window
{
    public static Clipboard Instance { get; private set; }
    private static PackedScene _entryPrefab = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/Clipboard/ClipboardEntry.tscn");
    private VBoxContainer _entries;
    private Label _tipLabel;
    private List<object> _values = [];
    public override void _Ready()
    {
        Instance = this;

        _entries = GetNode<VBoxContainer>("%EntryContainer");
        _tipLabel = GetNode<Label>("%TipLabel");
        
        CloseRequested += Hide;
    }

    public void AddEntry(object value)
    {
        _tipLabel.Visible = false;
        var entry = _entryPrefab.Instantiate<ClipboardEntry>();
        entry.SetValue(value, _values.Count);
        entry.Removed += OnEntryRemoved;

        _entries.AddChild(entry);
        _values.Add(value);
    }

    public bool TryGetValue(int index, out object value)
    {
        value = null;
        if (index < _values.Count)
        {
            value = _values[index];
            return true;
        }

        return false;
    }

    private void OnEntryRemoved(int index)
    {
        var entry = _entries.GetChild(index);
        _entries.RemoveChild(entry);
        entry.QueueFree();

        _values.RemoveAt(index);
        _tipLabel.Visible = _values.Count == 0;

        // 更新索引
        for (int i = index; i < _entries.GetChildCount(); i++)
        {
            var child = _entries.GetChild(i) as ClipboardEntry;
            child.SetValue(child.GetValue(), i);
        }
    }

}
