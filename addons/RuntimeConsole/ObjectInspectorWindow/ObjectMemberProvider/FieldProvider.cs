using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public class FieldProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        var type = obj.GetType();
        var attribute = type.GetCustomAttribute<ExtendedInspectorAttribute>();

        // 默认只显示实例和公共字段
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (attribute != null)
        {
            // 包含非公共成员
            if (attribute.IncludeNonPublic) flags |= BindingFlags.NonPublic;

            // 包含静态成员
            if (attribute.IncludeStatic) flags |= BindingFlags.Static;
        }

        foreach (var fieldInfo in type.GetFields(flags))
        {
            // 忽略标记为 HiddenInInspector 的字段
            if (fieldInfo.GetCustomAttribute<HiddenInInspectorAttribute>() != null)
                continue;

            var showInInspector = fieldInfo.GetCustomAttribute<ShowInInspectorAttribute>();

            // 跳过非公共、静态，且没有标记显示在检查器的字段
            if ((!fieldInfo.IsPublic || fieldInfo.IsStatic) && showInInspector == null)
                continue;

            var displayName = fieldInfo.Name;
            if (showInInspector != null && showInInspector.DisplayName != null)
                displayName = showInInspector.DisplayName; // 使用自定义名称

            // 字段也使用属性编辑器，因为它们在编辑方式上都是一样的
            var editor = PropertyEditorFactory.Create(fieldInfo.FieldType);
            editor.Name = displayName;

            editor.ValueChanged += (value) =>
            {                
                try { fieldInfo.SetValue(obj, value); }
                catch (Exception ex) { GD.PrintErr($"Failed to set field '{fieldInfo.Name}': {ex.Message}"); }
            };

            var value = fieldInfo.GetValue(obj);
            editor.SetMemberInfo(displayName, fieldInfo.FieldType, value, MemberEditorType.Field);
            yield return editor;
        }

    }
}