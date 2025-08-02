using Godot;

namespace RuntimeConsole;

/// <summary>
/// 命令接口，创建命令时实现此接口
/// </summary>
public interface IConsoleCommand
{
    /// <summary>
    /// 命令的关键字，用于在输出窗口识别
    /// </summary>
    string Keyword { get; }

    /// <summary>
    /// 命令所需的参数（按顺序）
    /// </summary>
    Variant.Type[] ParameterTypes { get; }

    /// <summary>
    /// 命令的执行行为
    /// </summary>
    /// <param name="parameters">用户输入解析后的参数</param>
    void Execute(Godot.Collections.Array parameters);
}
