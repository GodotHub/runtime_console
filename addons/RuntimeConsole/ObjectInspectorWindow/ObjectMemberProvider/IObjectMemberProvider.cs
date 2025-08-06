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
    /// <param name="context">上下文，用以提供额外信息</param>
    /// <returns>对象成员编辑器</returns>    
    IEnumerable<IMemberEditor> Populate(object obj, params object[] context);
}