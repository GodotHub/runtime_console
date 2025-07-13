using Godot;

namespace RuntimeConsole;

// JSON解析器会将数字转化为float类型！
public partial class JsonParser : Resource, IParameterParser
{
    public Variant.Type[] SupportedTypes => [Variant.Type.Dictionary, Variant.Type.Array];
    public Variant Result { get; private set; }

    public Error Parse(string token)
    {
        var json = Json.ParseString(token);
        if (json.VariantType == Variant.Type.Nil)
            return Error.InvalidParameter;

        Result = json;
        return Error.Ok;
    }
}