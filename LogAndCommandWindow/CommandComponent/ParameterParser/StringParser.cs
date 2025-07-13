using Godot;

namespace RuntimeConsole;

public partial class StringParser : Resource, IParameterParser
{
    public Variant.Type[] SupportedTypes => [Variant.Type.String];
    public Variant Result { get; private set; }

    public Error Parse(string token)
    {
        Result = token;
        return Error.Ok;
    }
}