using System;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public static class VariantUtility
{
    public static Variant Create(object obj)
    {        
        ArgumentNullException.ThrowIfNull(obj);

        var variantFromMethod = typeof(Variant).GetMethod("From", BindingFlags.Static | BindingFlags.Public)
            ?.MakeGenericMethod(obj.GetType());
        var variantValue = variantFromMethod?.Invoke(null, [obj]);
        return variantValue != null ? (Variant)variantValue : new Variant();
    }

    public static Type GetNativeType(this Variant value) => value.VariantType switch
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
        _ => typeof(object),
    };
}