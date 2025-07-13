using Godot;

namespace RuntimeConsole;

public partial class VariantParser : Resource, IParameterParser
{
    public Variant.Type[] SupportedTypes => [Variant.Type.Nil];

    public Variant Result { get; private set; }

    public Error Parse(string token)
    {
        Result = GD.StrToVar(token);
        return Error.Ok;
    }

}
