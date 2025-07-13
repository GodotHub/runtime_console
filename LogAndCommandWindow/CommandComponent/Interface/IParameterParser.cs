using Godot;
namespace RuntimeConsole;

/// <summary>
/// 命令参数分析器接口
/// </summary>
public interface IParameterParser
{
    /// <summary>
    /// 该分析器的支持的结果类型
    /// </summary>    
    Variant.Type[] SupportedTypes  { get; }
    /// <summary>
    /// 解析结果
    /// </summary>
    Variant Result { get; }
    /// <summary>
    /// 解析命令参数
    /// </summary>
    /// <param name="token">输入的命令参数</param>
    /// <returns>解析成功返回OK，解析失败返回InvalidParameter</returns>
    Error Parse(string token);
}