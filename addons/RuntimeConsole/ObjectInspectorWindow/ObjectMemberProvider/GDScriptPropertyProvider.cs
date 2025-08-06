using System;
using System.Collections.Generic;
using Godot;

namespace RuntimeConsole;

public class GDScriptPropertyProvider : IObjectMemberProvider
{
    // 只处理GDScript成员变量
    public IEnumerable<IMemberEditor> Populate(object obj)
    {
        if (obj is not GodotObject gdObj || gdObj.GetScript().Obj is not GDScript gdScript)
            yield break;

        var propertyList = gdScript.GetScriptPropertyList();
        GD.Print(propertyList);
        foreach (var prop in propertyList)
        {
            var propName = prop["name"].AsString();
            var usage = prop["usage"].As<PropertyUsageFlags>();
            var hint = prop["hint"].As<PropertyHint>();
            var type = prop["type"].As<Variant.Type>();
            var hintString = prop["hint_string"].AsString();

            // 跳过脚本导出类别属性（fuck godot，为什么你导出类别@export_category("Test")也是一个脚本的类成员）
            if ((usage & PropertyUsageFlags.ScriptVariable) == 0 &&
                ((usage & PropertyUsageFlags.Category) != 0 ||
                (usage & PropertyUsageFlags.Group) != 0 ||
                (usage & PropertyUsageFlags.Subgroup) != 0))
            {
                continue;
            }

            var propValue = gdObj.Get(propName);
            PropertyEditorBase editor = null;

            /*
            这里只有导出变量才能被识别为枚举，因为这样可以直接通过HintString获取到枚举值和枚举名，而不需要去使用GDScript表达式调用find_key、反射所有Godot枚举来获取枚举值和枚举名
            如果没有导出枚举变量或位标记变量，则直接当作普通的变量处理
            导出枚举也能用在字符串上，但是这里只处理整数的枚举变量，因为这是最常的用法：@export var my_enum = MyEnum.ONE; @export var my_enum: MyEnum
            将字符串以枚举导出得使用 @export_enum("ONE","TWO","THREE") var my_enum = "ONE"
            所以这里是在处理导出整数枚举，以防混淆保持一致性，不处理导出字符串枚举
            */
            if (hint == PropertyHint.Enum && type == Variant.Type.Int)
            {
                var enumValues = GetGDScriptEnum(hintString);
                if (enumValues.Count > 0)
                {
                    editor = PropertyEditorFactory.CreateEnumEditorForGDScript(hint);
                    // 这里传入一个元组交由枚举编辑器处理，Item1是枚举键值，Item2是当前的枚举值
                    editor.SetMemberInfo(propName, propValue.GetType(), (enumValues, propValue), MemberEditorType.Property);
                }

            }
            // 处理标志位
            else if (hint == PropertyHint.Flags && type == Variant.Type.Int)
            {
                var flagValues = GetGDScriptFlags(hintString);
                if (flagValues.Count > 0)
                {
                    editor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Flags);
                    editor.SetMemberInfo(propName, propValue.GetType(), (flagValues, propValue), MemberEditorType.Property);
                }
            }
            // 枚举数组
            else if (hint == PropertyHint.TypeString && type == Variant.Type.Array)
            {

            }
            else
            {
                editor = PropertyEditorFactory.Create(propValue.GetType());
                editor.SetMemberInfo(propName, propValue.GetType(), propValue, MemberEditorType.Property);
            }


            editor.SetEditable(true);
            editor.ValueChanged += (value) =>
            {
                try
                {
                    Variant variant = VariantUtility.Create(value);
                    gdObj.Set(propName, variant);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to set GDScript property '{propName}': {ex.Message}");
                }
            };

            yield return editor;
        }
    }

    private Dictionary<string, int> GetGDScriptFlags(string hintString)
    {
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

    private Dictionary<string, int> GetGDScriptEnum(string hintString)
    {
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