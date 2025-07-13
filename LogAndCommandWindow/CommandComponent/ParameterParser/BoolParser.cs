using Godot;

namespace RuntimeConsole;

public partial class BoolParser : Resource, IParameterParser
{
    public Variant.Type[] SupportedTypes => [Variant.Type.Bool];
    public Variant Result { get; private set; }
    public Error Parse(string token)
    {
        if (bool.TryParse(token, out var result))
        {
            Result = result;
            return Error.Ok;
        }

        return Error.InvalidParameter;
    }
}