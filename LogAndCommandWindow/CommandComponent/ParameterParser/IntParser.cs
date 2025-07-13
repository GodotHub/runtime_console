using Godot;

namespace RuntimeConsole;

public partial class IntParser : Resource, IParameterParser
{
    public Variant.Type[] SupportedTypes => [Variant.Type.Int];
    public Variant Result { get; private set; }

    public Error Parse(string token)
    {
        if (long.TryParse(token, out var result))
        {
            Result = result;
            return Error.Ok;
        }

        return Error.InvalidParameter;
    }
}