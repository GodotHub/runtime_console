using System;

namespace RuntimeConsole;

/// <summary>
/// 请求展开对象，用于Object类型属性编辑器
/// </summary>
public interface IExpendObjectRequester
{
    /// <summary>
    /// 请求创建新的ObjectMemberPanel
    /// </summary>
    event Action<object, string> RequestCreateNewPanel;
}