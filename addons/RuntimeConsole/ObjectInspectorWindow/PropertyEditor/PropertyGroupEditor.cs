using System;
using System.Collections.Generic;

namespace RuntimeConsole;

/// <summary>
/// 属性组编辑器，用作向量类型、矩阵类型的编辑器基类
/// </summary>
public abstract partial class PropertyGroupEditor : PropertyEditorBase
{
    /// <summary>
    /// 获取子属性
    /// </summary>
    /// <returns>返回向量类型的数值编辑器</returns>
    public abstract IEnumerable<PropertyEditorBase> GetChildProperties();

    /// <summary>
    /// 当子属性值改变时调用
    /// </summary>    
    protected abstract void OnChildValueChanged(PropertyEditorBase sender, object value);

}