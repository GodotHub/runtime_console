using Godot;

namespace RuntimeConsole;
public enum MemberEditorType
{
    Property,
    Field,
    Method,
    Event
}
public interface IMemberEditor
{
    /// <summary>
    /// 成员名称
    /// </summary>
    string MemberName { get; }

    /// <summary>
    /// 成员类型
    /// </summary>
    MemberEditorType MemberType { get; }

    /// <summary>
    /// 转为控件
    /// </summary>
    Control AsControl();
    
}