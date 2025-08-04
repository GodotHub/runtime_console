using System.Collections.Generic;

namespace RuntimeConsole;

/// <summary>
/// 对象成员提供者
/// </summary>
public interface IObjectMemberProvider
{
    /// <summary>
    /// 反射获取对象成员，并返回填充数据后的编辑器
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns>对象成员编辑器</returns>
    /// </summary>    
    IEnumerable<IMemberEditor> Populate(object obj);
}