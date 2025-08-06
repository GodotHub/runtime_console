using System.Collections.Generic;

namespace RuntimeConsole;

public static class GDScriptUtility
{
    /// <summary>
    /// 将传入的HintString转换为位标志键值字典
    /// </summary>
    /// <param name="hintString">GDScript属性的提示字符串</param>
    /// <returns>由该提示字符串解析而来的位标志键值字典，解析失败返回空字典</returns>
    public static Dictionary<string, int> GetGDScriptFlags(string hintString)
    {
        if (string.IsNullOrEmpty(hintString))
            return new Dictionary<string, int>();

        if (hintString.StartsWith("2/6:"))
            hintString = hintString.Substring(4);
            
        // "Bit0,Bit1,Bit3,Bit4"
        var bits = hintString.Split(',');
        var flagsDict = new Dictionary<string, int>();

        // 位标志不会受前一显式值的影响
        var autoValue = 1;
        foreach (var bit in bits)
        {
            // "Bit0:1, Bit1:4, Bit2:8"
            var key = bit;
            var kv = bit.Split(':');
            var value = autoValue;
            if (kv.Length == 2)
            {
                key = kv[0];
                value = int.Parse(kv[1]);
            }
            flagsDict.TryAdd(key, value);
            autoValue <<= 1;
        }

        return flagsDict;
    }

    /// <summary>
    /// 将传入的来自类型化枚举字典属性的HintString转换为枚举键值字典
    /// </summary>
    /// <param name="hintString">GDScript属性的提示字符串</param>
    /// <returns>由该提示字符串解析而来的枚举键值字典，解析失败返回空字典</returns>
    public static Dictionary<string, int> GetGDScriptDictionaryValueEnum(string hintString)
    {
        // "KeyType;2/2:EnumName1,EnumName2:Value1,EnumName2:Value2"
        var valueParts = hintString.Split(';');
        if (valueParts.Length != 2)
            return new Dictionary<string, int>();

        var valuePart = valueParts[1];
        if (valuePart.StartsWith("2/2:"))
            valuePart = valuePart.Substring(4);

        return GetGDScriptEnum(valuePart);

    }

    /// <summary>
    /// 将传入的HintString转换为枚举键值字典
    /// </summary>
    /// <param name="hintString">GDScript属性的提示字符串</param>
    /// <returns>由该提示字符串解析而来的枚举键值字典，解析失败返回空字典</returns>
    public static Dictionary<string, int> GetGDScriptEnum(string hintString)
    {
        if (string.IsNullOrEmpty(hintString))
            return new Dictionary<string, int>();

        if (hintString.StartsWith("2/2:"))
            hintString = hintString.Substring(4);

        // "Hello,World,Goodbye,World"
        var keys = hintString.Split(',');
        var enumDict = new Dictionary<string, int>();

        // 枚举受前一个显式值的影响
        var value = 0;
        foreach (var k in keys)
        {
            // "Hello:1,World:2,Goodbye:3,World:4"
            var key = k;
            var kv = k.Split(':');
            if (kv.Length == 2)
            {
                key = kv[0];
                value = int.Parse(kv[1]);
            }
            enumDict.TryAdd(key, value);
            value++;
        }

        return enumDict;
    }
}