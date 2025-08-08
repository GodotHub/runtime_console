using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

public partial class VectorPropertyEditor : PropertyGroupEditor
{
    private VBoxContainer _children;

    private object _vectorValue;
    private object _tempValue;
    
    protected override void OnSceneInstantiated()
    {

        base.OnSceneInstantiated();
        _children = GetNode<VBoxContainer>("%Children");
    }

    protected override IEnumerable<PropertyEditorBase> GetChildProperties()
    {
        return _children.GetChildren().Cast<PropertyEditorBase>();
    }

    public override object GetValue()
    {
        return _vectorValue;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
        _editButton.Disabled = !editable;
        foreach (var child in GetChildProperties())
        {
            child.SetEditable(editable);
        }
    }

    protected override void OnChildValueChanged(PropertyEditorBase sender, object value)
    {
        _tempValue = _vectorValue;
        switch (_vectorValue)
        {
            case Vector2 v2 when value is float v2NewValue:
                _tempValue = UpdateVector2(v2, sender.MemberName, v2NewValue);
                break;

            case Vector2I v2i when value is int v2iNewValue:
                _tempValue = UpdateVector2I(v2i, sender.MemberName, v2iNewValue);
                break;

            case Vector3 v3 when value is float v3NewValue:
                _tempValue = UpdateVector3(v3, sender.MemberName, v3NewValue);
                break;

            case Vector4 v4 when value is float v4NewValue:
                _tempValue = UpdateVector4(v4, sender.MemberName, v4NewValue);
                break;

            case Vector4I v4i when value is int v4iNewValue:
                _tempValue = UpdateVector4I(v4i, sender.MemberName, v4iNewValue);
                break;

            case Quaternion q when value is float qNewValue:
                _tempValue = UpdateQuaternion(q, sender.MemberName, qNewValue);
                break;

            case Aabb aabb:
                _tempValue = UpdateAabb(aabb, sender.MemberName, value);
                break;

            case Basis b:
                _tempValue = UpdateBasis(b, sender.MemberName, value);
                break;

            case Plane p:
                _tempValue = UpdatePlane(p, sender.MemberName, value);
                break;

            case Rect2 r:
                _tempValue = UpdateRect2(r, sender.MemberName, value);
                break;

            case Rect2I ri:
                _tempValue = UpdateRect2I(ri, sender.MemberName, value);
                break;

            case Transform2D t:
                _tempValue = UpdateTransform2D(t, sender.MemberName, value);
                break;

            case Transform3D t3:
                _tempValue = UpdateTransform3D(t3, sender.MemberName, value);
                break;
        }
    }
    #region 更新向量
    private Vector2 UpdateVector2(Vector2 v, string memberName, float newValue) => memberName switch
    {
        "X" => new Vector2(newValue, v.Y),
        "Y" => new Vector2(v.X, newValue),
        _ => v
    };

    private Vector2I UpdateVector2I(Vector2I v, string memberName, int newValue) => memberName switch
    {
        "X" => new Vector2I(newValue, v.Y),
        "Y" => new Vector2I(v.X, newValue),
        _ => v
    };

    private Vector3 UpdateVector3(Vector3 v, string memberName, float newValue) => memberName switch
    {
        "X" => new Vector3(newValue, v.Y, v.Z),
        "Y" => new Vector3(v.X, newValue, v.Z),
        "Z" => new Vector3(v.X, v.Y, newValue),
        _ => v
    };

    private Vector4 UpdateVector4(Vector4 v, string memberName, float newValue) => memberName switch
    {
        "X" => new Vector4(newValue, v.Y, v.Z, v.W),
        "Y" => new Vector4(v.X, newValue, v.Z, v.W),
        "Z" => new Vector4(v.X, v.Y, newValue, v.W),
        "W" => new Vector4(v.X, v.Y, v.Z, newValue),
        _ => v
    };

    private Vector4I UpdateVector4I(Vector4I v, string memberName, int newValue) => memberName switch
    {
        "X" => new Vector4I(newValue, v.Y, v.Z, v.W),
        "Y" => new Vector4I(v.X, newValue, v.Z, v.W),
        "Z" => new Vector4I(v.X, v.Y, newValue, v.W),
        "W" => new Vector4I(v.X, v.Y, v.Z, newValue),
        _ => v
    };

    private Quaternion UpdateQuaternion(Quaternion q, string memberName, float newValue) => memberName switch
    {
        "X" => new Quaternion(newValue, q.Y, q.Z, q.W),
        "Y" => new Quaternion(q.X, newValue, q.Z, q.W),
        "Z" => new Quaternion(q.X, q.Y, newValue, q.W),
        "W" => new Quaternion(q.X, q.Y, q.Z, newValue),
        _ => q
    };

    private Aabb UpdateAabb(Aabb aabb, string memberName, object value) => memberName switch
    {
        "Position" when value is Vector3 newPosition => new Aabb(newPosition, aabb.Size),
        "Size" when value is Vector3 newSize => new Aabb(aabb.Position, newSize),
        _ => aabb
    };

    private Basis UpdateBasis(Basis b, string memberName, object value) => memberName switch
    {
        "Column0" when value is Vector3 newColumn0 => new Basis(newColumn0, b.Column1, b.Column2),
        "Column1" when value is Vector3 newColumn1 => new Basis(b.Column0, newColumn1, b.Column2),
        "Column2" when value is Vector3 newColumn2 => new Basis(b.Column0, b.Column1, newColumn2),
        _ => b
    };

    private Plane UpdatePlane(Plane p, string memberName, object value) => memberName switch
    {
        "Normal" when value is Vector3 newNormal => new Plane(newNormal, p.D),
        "D" when value is float newD => new Plane(p.Normal, newD),
        _ => p
    };

    private Rect2 UpdateRect2(Rect2 r, string memberName, object value) => memberName switch
    {
        "Position" when value is Vector2 newPosition => new Rect2(newPosition, r.Size),
        "Size" when value is Vector2 newSize => new Rect2(r.Position, newSize),
        _ => r
    };

    private Rect2I UpdateRect2I(Rect2I ri, string memberName, object value) => memberName switch
    {
        "Position" when value is Vector2I newPosition => new Rect2I(newPosition, ri.Size),
        "Size" when value is Vector2I newSize => new Rect2I(ri.Position, newSize),
        _ => ri
    };

    private Transform2D UpdateTransform2D(Transform2D t, string memberName, object value) => memberName switch
    {
        "X" when value is Vector2 newX => new Transform2D(newX, t.Y, t.Origin),
        "Y" when value is Vector2 newY => new Transform2D(t.X, newY, t.Origin),
        "Origin" when value is Vector2 newOrigin => new Transform2D(t.X, t.Y, newOrigin),
        _ => t
    };

    private Transform3D UpdateTransform3D(Transform3D t, string memberName, object value) => memberName switch
    {
        "Basis" when value is Basis newBasis => new Transform3D(newBasis, t.Origin),
        "Origin" when value is Vector3 newOrigin => new Transform3D(t.Basis, newOrigin),
        _ => t
    };
    #endregion
    protected override void OnSubmission()
    {
        if (Editable)
        {
            if (_tempValue != null && !_tempValue.Equals(_vectorValue))
            {
                _vectorValue = _tempValue;
                NotificationValueChanged();
            }
        }
    }

    protected override void SetValue(object value)
    {
        _vectorValue = value;

        switch (value)
        {
            case Vector2 v2:
                CreateChildEditor("X", v2.X);
                CreateChildEditor("Y", v2.Y);
                break;
            case Vector3 v3:
                CreateChildEditor("X", v3.X);
                CreateChildEditor("Y", v3.Y);
                CreateChildEditor("Z", v3.Z);
                break;
            case Vector4 v4:
                CreateChildEditor("X", v4.X);
                CreateChildEditor("Y", v4.Y);
                CreateChildEditor("Z", v4.Z);
                CreateChildEditor("W", v4.W);
                break;
            case Vector2I v2i:
                CreateChildEditor("X", v2i.X);
                CreateChildEditor("Y", v2i.Y);
                break;
            case Vector3I v3i:
                CreateChildEditor("X", v3i.X);
                CreateChildEditor("Y", v3i.Y);
                CreateChildEditor("Z", v3i.Z);
                break;
            case Vector4I v4i:
                CreateChildEditor("X", v4i.X);
                CreateChildEditor("Y", v4i.Y);
                CreateChildEditor("Z", v4i.Z);
                CreateChildEditor("W", v4i.W);
                break;
            case Quaternion q:
                CreateChildEditor("X", q.X);
                CreateChildEditor("Y", q.Y);
                CreateChildEditor("Z", q.Z);
                CreateChildEditor("W", q.W);
                break;
            case Aabb aabb:
                CreateChildEditor("Size", aabb.Size);
                CreateChildEditor("Position", aabb.Position);
                break;
            case Basis b:
                CreateChildEditor("Column0", b.Column0);
                CreateChildEditor("Column1", b.Column1);
                CreateChildEditor("Column2", b.Column2);
                CreateChildEditor("Scale", b.Scale, false);
                break;
            case Plane p:
                CreateChildEditor("Normal", p.Normal);
                CreateChildEditor("D", p.D);
                break;
            case Rect2 r:
                CreateChildEditor("Position", r.Position);
                CreateChildEditor("Size", r.Size);
                CreateChildEditor("Area", r.Area, false);
                break;
            case Rect2I ri:
                CreateChildEditor("Position", ri.Position);                
                CreateChildEditor("Size", ri.Size);                
                CreateChildEditor("Area", ri.Area, false);
                break;
            case Transform2D t:
                CreateChildEditor("X", t.X);
                CreateChildEditor("Y", t.Y);
                CreateChildEditor("Origin", t.Origin);
                CreateChildEditor("Rotation", t.Rotation, false);
                CreateChildEditor("Scale", t.Scale, false);
                CreateChildEditor("Skew", t.Skew, false);
                break;
            case Transform3D t3:
                CreateChildEditor("Basis", t3.Basis);
                CreateChildEditor("Origin", t3.Origin);
                break;

        }
    }

    // 创建数值属性编辑器
    private void CreateChildEditor(string name, object value, bool editable = true)
    {
        var editor = PropertyEditorFactory.Create(value.GetType());
        editor.SetMemberInfo(name, value.GetType(), value, MemberType);
        editor.ValueChanged += (value) => OnChildValueChanged(editor, value);
        editor.SetEditable(editable);
        _children.AddChild(editor);
    }
}
