using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public class PropertyProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        var type = obj.GetType();
        var attribute = type.GetCustomAttribute<ExtendedInspectorAttribute>();

        // 默认只显示实例和公共属性
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (attribute != null)
        {
            // 包含非公共成员
            if (attribute.IncludeNonPublic) flags |= BindingFlags.NonPublic;

            // 包含静态成员
            if (attribute.IncludeStatic) flags |= BindingFlags.Static;
        }

        foreach (var propertyInfo in type.GetProperties(flags))
        {
            // 忽略无法读取的属性
            if (!propertyInfo.CanRead)
                continue;

            // 跳过索引器属性
            if (propertyInfo.GetIndexParameters().Length > 0) continue;

            // 忽略标记为 HiddenInInspector 的属性
            if (propertyInfo.GetCustomAttribute<HiddenInInspectorAttribute>() != null)
                continue;

            var getter = propertyInfo.GetGetMethod(true); // 获取非公共的 getter 方法
            bool isStatic = getter?.IsStatic == true; // 判断是否是静态方法，即静态属性
            bool isNonPublic = getter != null && !getter.IsPublic; // 判断是否是非公共属性
            var showInInspector = propertyInfo.GetCustomAttribute<ShowInInspectorAttribute>();

            // 属性是非公共或静态的，且没有显示指定在Inspector中显示，跳过
            if ((isNonPublic || isStatic) && showInInspector == null)
                continue;

            var displayName = propertyInfo.Name;
            if (showInInspector != null && showInInspector.DisplayName != null)
                displayName = showInInspector.DisplayName; // 使用自定义显示名称，如果有

            var editor = PropertyEditorFactory.Create(propertyInfo.PropertyType);
            editor.Name = displayName;            

            bool isEditable = propertyInfo.CanWrite;
            editor.SetEditable(isEditable);

            if (isEditable)
            {
                editor.ValueChanged += (value) =>
                {
                    try { propertyInfo.SetValue(obj, value); }
                    catch (Exception ex) { GD.PrintErr($"Failed to set property '{propertyInfo.Name}': {ex.Message}"); }
                };
            }

            var value = propertyInfo.GetValue(obj);
            editor.SetMemberInfo(displayName, propertyInfo.PropertyType, value, MemberEditorType.Property);
            yield return editor;
        }
    }
}