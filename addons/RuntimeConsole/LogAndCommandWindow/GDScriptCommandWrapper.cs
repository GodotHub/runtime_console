using System.Linq;
using Godot;

namespace RuntimeConsole;

public class GDScriptCommandWrapper(GDScript script) : IConsoleCommand
{
    private readonly GodotObject _instance = script.New().AsGodotObject();

    public string Keyword => _instance.HasMethod("get_keyword")
        ? _instance.Call("get_keyword").As<string>()
        : _instance.Get("keyword").As<string>();

    public Variant.Type[] ParameterTypes => _instance.HasMethod("get_parameter_types")
        ? _instance.Call("get_parameter_types").AsGodotArray<Variant.Type>().ToArray()
        : _instance.Get("parameter_types").AsGodotArray<Variant.Type>().ToArray();

    public void Execute(Godot.Collections.Array args) => _instance.Call("execute", args);
}