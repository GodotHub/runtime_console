using System;

namespace RuntimeConsole;

public delegate void RequestCreateNewPanelEventHandler(PropertyEditorBase sender, object obj);

/// <summary>
/// 请求展开对象，用于Object类型属性编辑器
/// </summary>
public interface IExpendObjectRequester
{
    /// <summary>
    /// 请求创建新的ObjectMemberPanel
    /// </summary>
    event RequestCreateNewPanelEventHandler CreateNewPanelRequested;

    /// <summary>
    /// 当创建完成后的回调
    /// </summary>
    /// <param name="panel">新面板的引用</param>
    void OnPanelCreated(ObjectMemberPanel panel);
}