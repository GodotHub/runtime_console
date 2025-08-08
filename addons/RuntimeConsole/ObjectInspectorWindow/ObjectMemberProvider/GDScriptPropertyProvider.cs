using System;
using System.Collections.Generic;
using Godot;

namespace RuntimeConsole;

public class GDScriptPropertyProvider : IObjectMemberProvider
{
    // 只处理GDScript成员变量
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        if (obj is not GodotObject gdObj || gdObj?.GetScript().Obj is not GDScript gdScript)
            yield break;

        var propertyList = gdScript.GetScriptPropertyList();
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
            PropertyEditorBase editor = PropertyEditorFactory.Create(propValue.GetType());

            // 对于枚举数组/字典的情况，传递上下文，
            // 这里始终为true，VariantPropertyEditor也实现了这个接口，它会传递给CollectionPropertyEditor，最后交由ElementProvider处理，然后又传回VariantPropertyEditor处理
            if (editor is IExpendObjectRequester requester)
            {
                requester.SetContext([prop]);
            }

            // 这里的调用顺序绝对不能改！要在设置上下文之后再设置属性信息！
            editor.SetMemberInfo(propName, typeof(Variant), propValue, MemberEditorType.Property);
            
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