using System.Linq;
using Godot;

namespace RuntimeConsole;

public class GDScriptParserWrapper(GDScript script) : IParameterParser
{    
    private readonly GodotObject _instance = script.New().AsGodotObject();

    public Variant.Type[] SupportedTypes => _instance.HasMethod("get_supported_types")
        ? _instance.Call("get_supported_types").AsGodotArray<Variant.Type>().ToArray() 
        : _instance.Get("supported_types").AsGodotArray<Variant.Type>().ToArray();

    public Variant Result => _instance.HasMethod("get_result") 
        ? _instance.Call("get_result") 
        : _instance.Get("result");

    public Error Parse(string input) => _instance.Call("parse", input).As<Error>();
}