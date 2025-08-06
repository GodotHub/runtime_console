using System;
using System.Collections.Generic;
using Godot;

namespace RuntimeConsole;

public class GDScriptPropertyProvider : IObjectMemberProvider
{
    // 只处理GDScript成员变量
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        if (obj is not GodotObject gdObj || gdObj.GetScript().Obj is not GDScript gdScript)
            yield break;

        var propertyList = gdScript.GetScriptPropertyList();
        // GD.Print(propertyList);
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

            // 注意：若枚举中存在多个枚举项具有相同的常量值，C# 反查枚举名时无法保证解析出的名称是否为用户实际书写的那个枚举名。
            // 反查操作只会返回第一个与该值匹配的枚举项名称。
            // 因此，在处理枚举数组或枚举字典时，若存在值重复的枚举项，将无法确保UI展示的是用户期望的枚举名。
            /*
            这里只有导出变量才能被识别为枚举，因为这样可以直接通过HintString获取到枚举值和枚举名，而不需要去使用GDScript表达式调用find_key、反射所有Godot枚举来获取枚举值和枚举名
            如果没有导出枚举变量或位标记变量，则直接当作普通的变量处理
            导出枚举也能用在字符串上，但是这里只处理整数的枚举变量，因为这是最常的用法：@export var my_enum = MyEnum.ONE; @export var my_enum: MyEnum
            将字符串以枚举导出得使用 @export_enum("ONE","TWO","THREE") var my_enum = "ONE"
            所以这里是在处理导出整数枚举，以防混淆保持一致性，不处理导出字符串枚举
            */
            if (hint == PropertyHint.Enum && type == Variant.Type.Int)
            {
                var enumValues = GDScriptUtility.GetGDScriptEnum(hintString);
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
                var flagValues = GDScriptUtility.GetGDScriptFlags(hintString);
                if (flagValues.Count > 0)
                {
                    editor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Flags);
                    editor.SetMemberInfo(propName, propValue.GetType(), (flagValues, propValue), MemberEditorType.Property);
                }
            }
            // 默认情况，工厂构造VariantPropertyEditor，根据Variant类型自动分派编辑器
            else
            {
                editor = PropertyEditorFactory.Create(propValue.GetType());

                // 对于枚举数组/字典的情况，传递上下文，
                // 这里始终为true，VariantPropertyEditor也实现了这个接口，它会传递给CollectionPropertyEditor，最后交由ElementProvider处理
                if (editor is IExpendObjectRequester requester)
                {
                    requester.SetContext([prop]);
                }

                // 这里的调用顺序绝对不能改！要在设置上下文之后再设置属性信息！
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

}