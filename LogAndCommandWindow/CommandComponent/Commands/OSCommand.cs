using Godot;
using RuntimeConsole;

partial class OSCommand : Resource, IConsoleCommand
{
    public string Keyword => "os";

    public Variant.Type[] ParameterTypes => [];

    public void Execute(Godot.Collections.Array args)
    {        
        var log = Console.GameConsole.GetConsoleWindow<LogCommandWindow>("Log and Command");        
        var memInfo = OS.GetMemoryInfo();
        var info =
$@"OS: {OS.GetName()} {OS.GetVersionAlias()}
GPU: {RenderingServer.GetVideoAdapterName()}
CPU: {OS.GetProcessorName()}
Number of CPU cores: {OS.GetProcessorCount()}
RAM: {memInfo["physical"].AsInt64() / 1024.0 / 1024.0 / 1024.0:F2}GB";
        log.PrintRaw(info);
    }
}