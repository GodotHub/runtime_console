using Godot;
using System;
using System.Collections;
using System.Reflection;

namespace RuntimeConsole;

/// <summary>
/// 属性编辑器工厂类，用于根据属性类型创建相应的编辑器实例
/// </summary>
public static class PropertyEditorFactory
{
    /// <summary>
    /// 根据属性类型创建对应的属性编辑器实例
    /// </summary>
    /// <param name="propertyType">属性类型</param>
    /// <returns>对应的属性编辑器实例</returns>
    public static PropertyEditorBase Create(Type propertyType)
    {
        ArgumentNullException.ThrowIfNull(propertyType);

        if (propertyType == typeof(char))
        {
            return CreateInstance<CharPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/CharPropertyEditor.tscn");
        }

        // 布尔类型属性编辑器
        if (propertyType == typeof(bool))
        {
            return CreateInstance<BoolPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/BoolPropertyEditor.tscn");
        }

        // 枚举类型属性编辑器
        if (propertyType.IsEnum)
        {
            if (propertyType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                return CreateInstance<FlagPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/FlagPropertyEditor.tscn");
            }
            else
            {
                return CreateInstance<EnumPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/EnumPropertyEditor.tscn");
            }
        }

        // 数值类型属性编辑器
        if (IsNumericType(propertyType))
        {
            return CreateInstance<NumberPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/NumberPropertyEditor.tscn");
        }

        // 字符串类型属性编辑器
        if (propertyType == typeof(string) || propertyType == typeof(StringName) || propertyType == typeof(NodePath))
        {
            return CreateInstance<StringPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/StringPropertyEditor.tscn");
        }

        // 颜色编辑器
        if (propertyType == typeof(Color))
        {
            return CreateInstance<ColorPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/ColorPropertyEditor.tscn");
        }

        // 向量/矩阵编辑器
        if (IsVectorType(propertyType))
        {
            return CreateInstance<VectorPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/VectorPropertyEditor.tscn");
        }

        // 集合类型
        if (typeof(IEnumerable).IsAssignableFrom(propertyType))
        {
            return CreateInstance<CollectionPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/CollectionPropertyEditor.tscn");
        }        

        if (propertyType == typeof(Variant))
        {
            // 使用特殊的Variant类型编辑器来创建对应的C#类型编辑器
            return new VariantPropertyEditor();
        }

        // 通用值类型编辑器
        if (propertyType.IsValueType)
        {
            return CreateInstance<StructPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/StructPropertyEditor.tscn");
        }
        

        // 默认使用对象属性编辑器
        return CreateInstance<ObjectPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/ObjectPropertyEditor.tscn");
    }

    public static PropertyEditorBase CreateEnumEditorForGDScript(PropertyHint hint)
    {
        if (hint == PropertyHint.Enum)
        {
            return CreateInstance<EnumPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/EnumPropertyEditor.tscn");
        }

        if (hint == PropertyHint.Flags)
        {
            return CreateInstance<FlagPropertyEditor>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/PropertyEditor/FlagPropertyEditor.tscn");
        }

        throw new ArgumentException($"Invalid hint: {hint}, Expected: Enum or Flags");
    }

    /// <summary>
    /// 从场景文件创建实例
    /// </summary>
    /// <typeparam name="T">编辑器类型</typeparam>
    /// <param name="path">场景文件路径</param>
    /// <returns>实例化的编辑器</returns>
    private static T CreateInstance<T>(string path) where T : PropertyEditorBase
    {
        var scene = GD.Load<PackedScene>(path);
        return scene.Instantiate<T>();
    }

    /// <summary>
    /// 判断是否为数值类型
    /// </summary>
    /// <param name="type">要判断的类型</param>
    /// <returns>是否为数值类型</returns>
    private static bool IsNumericType(Type type)
    {
        return type == typeof(sbyte) || type == typeof(byte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }

    private static bool IsVectorType(Type type)
    {
        return type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
               type == typeof(Vector2I) || type == typeof(Vector3I) || type == typeof(Vector4I) ||
               type == typeof(Quaternion) || type == typeof(Rect2) || type == typeof(Rect2I) ||
               type == typeof(Aabb) || type == typeof(Plane) || type == typeof(Basis) ||
               type == typeof(Transform2D) || type == typeof(Transform3D);
    }
}