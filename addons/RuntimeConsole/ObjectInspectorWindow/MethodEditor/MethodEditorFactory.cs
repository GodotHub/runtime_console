using System.Reflection;
using Godot;

namespace RuntimeConsole;

public static class MethodEditorFactory
{
    private static readonly PackedScene _methodEditorScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MethodEditor/MethodEditor.tscn");
    public static MethodEditor Create(MethodInfo methodInfo)
    {
        var editor = _methodEditorScene.Instantiate<MethodEditor>();
        editor.SetMethodInfo(methodInfo.Name, methodInfo.ToString(), methodInfo.GetParameters().Length);
        return editor;
    }
}