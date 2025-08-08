using System;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public static class VariantUtility
{
    public static Variant Create(object obj)
    {        
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            var variantFromMethod = typeof(Variant).GetMethod("From", BindingFlags.Static | BindingFlags.Public)
                ?.MakeGenericMethod(obj.GetType());
            var variantValue = variantFromMethod?.Invoke(null, [obj]);
            return variantValue != null ? (Variant)variantValue : new Variant();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to create variant from object: {ex.Message}");
            return new Variant();
        }
    }

    /// <summary>
    /// 根据hint中的类型提示字符串获取对应的C#类型
    /// </summary>
    /// <param name="hint">属性元数据的hint提示字符串</param>
    /// <returns>匹配除Variant以外的所有Godot类型，默认返回int，指示属性是一个枚举，因为字符串Variant不可能出现在hint字符串中</returns>
    public static Type GetNativeType(string hint) => hint switch
    {
        "bool" => typeof(bool),
        "int" => typeof(long),
        "float" => typeof(double),
        "String" => typeof(string),
        "Vector2" => typeof(Vector2),
        "Vector2i" => typeof(Vector2I),
        "Rect2" => typeof(Rect2),
        "Rect2i" => typeof(Rect2I),
        "Vector3" => typeof(Vector3),
        "Vector3i" => typeof(Vector3I),
        "Transform2D" => typeof(Transform2D),
        "Vector4" => typeof(Vector4),
        "Vector4i" => typeof(Vector4I),
        "Plane" => typeof(Plane),
        "Quaternion" => typeof(Quaternion),
        "AABB" => typeof(Aabb),
        "Basis" => typeof(Basis),
        "Transform3D" => typeof(Transform3D),
        "Projection" => typeof(Projection),
        "Color" => typeof(Color),
        "StringName" => typeof(StringName),
        "NodePath" => typeof(NodePath),
        "RID" => typeof(Rid),
        "Object" => typeof(GodotObject),
        "Callable" => typeof(Callable),
        "Signal" => typeof(Signal),
        "Dictionary" => typeof(Godot.Collections.Dictionary),
        "Array" => typeof(Godot.Collections.Array),
        "PackedByteArray" => typeof(byte[]),
        "PackedInt32Array" => typeof(int[]),
        "PackedInt64Array" => typeof(long[]),
        "PackedFloat32Array" => typeof(float[]),
        "PackedFloat64Array" => typeof(double[]),
        "PackedStringArray" => typeof(string[]),
        "PackedVector2Array" => typeof(Vector2[]),
        "PackedVector3Array" => typeof(Vector3[]),
        "PackedColorArray" => typeof(Color[]),
        "PackedVector4Array" => typeof(Vector4[]),
        _ => Type.GetType(hint, false) ?? typeof(long) // hint中没有Variant这个类型提示，其他情况默认为整数（识别为枚举）
    };

    public static Type GetNativeType(Variant.Type type) => type switch
    {
        Variant.Type.Bool => typeof(bool),
        Variant.Type.Int => typeof(long),
        Variant.Type.Float => typeof(double),
        Variant.Type.String => typeof(string),
        Variant.Type.Vector2 => typeof(Vector2),
        Variant.Type.Vector2I => typeof(Vector2I),
        Variant.Type.Rect2 => typeof(Rect2),
        Variant.Type.Rect2I => typeof(Rect2I),
        Variant.Type.Vector3 => typeof(Vector3),
        Variant.Type.Vector3I => typeof(Vector3I),
        Variant.Type.Transform2D => typeof(Transform2D),
        Variant.Type.Vector4 => typeof(Vector4),
        Variant.Type.Vector4I => typeof(Vector4I),
        Variant.Type.Plane => typeof(Plane),
        Variant.Type.Quaternion => typeof(Quaternion),
        Variant.Type.Aabb => typeof(Aabb),
        Variant.Type.Basis => typeof(Basis),
        Variant.Type.Transform3D => typeof(Transform3D),
        Variant.Type.Projection => typeof(Projection),
        Variant.Type.Color => typeof(Color),
        Variant.Type.StringName => typeof(StringName),
        Variant.Type.NodePath => typeof(NodePath),
        Variant.Type.Rid => typeof(Rid),
        Variant.Type.Object => typeof(GodotObject),
        Variant.Type.Callable => typeof(Callable),
        Variant.Type.Signal => typeof(Signal),
        Variant.Type.Dictionary => typeof(Godot.Collections.Dictionary),
        Variant.Type.Array => typeof(Godot.Collections.Array),
        Variant.Type.PackedByteArray => typeof(byte[]),
        Variant.Type.PackedInt32Array => typeof(int[]),
        Variant.Type.PackedInt64Array => typeof(long[]),
        Variant.Type.PackedFloat32Array => typeof(float[]),
        Variant.Type.PackedFloat64Array => typeof(double[]),
        Variant.Type.PackedStringArray => typeof(string[]),
        Variant.Type.PackedVector2Array => typeof(Vector2[]),
        Variant.Type.PackedVector3Array => typeof(Vector3[]),
        Variant.Type.PackedColorArray => typeof(Color[]),
        Variant.Type.PackedVector4Array => typeof(Vector4[]),
        _ => null,
    };
}