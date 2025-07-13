using Godot;
namespace RuntimeConsole;

public partial class FloatParser : Resource, IParameterParser
{
    public Variant.Type[] SupportedTypes => [Variant.Type.Float];
    public Variant Result { get; private set; }

    public Error Parse(string token)
    {
        if (double.TryParse(token, out var result))
        {
            Result = result;
            return Error.Ok;
        }

        return Error.InvalidParameter;
    }
}