using System;
using System.Collections.Generic;

namespace RuntimeConsole;

/// <summary>
/// 属性组编辑器，用作结构体编辑器基类
/// </summary>
public abstract partial class PropertyGroupEditor : PropertyEditorBase
{
    /// <summary>
    /// 获取子属性
    /// </summary>
    /// <returns>返回该结构体的子属性编辑器</returns>
    protected abstract IEnumerable<PropertyEditorBase> GetChildProperties();

    /// <summary>
    /// 当子属性值改变时调用
    /// </summary>    
    protected abstract void OnChildValueChanged(PropertyEditorBase sender, object value);

}