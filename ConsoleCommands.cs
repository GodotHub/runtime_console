using Godot;

namespace RuntimeConsole;

[HideInObjectTree]
public partial class ConsoleCommands : Node
{
    public static ConsoleCommands Commands { get; private set; }

    public override void _Ready()
    {
        Commands = this;
    }
    /*
        **The method of adding commands may be changed in future versions.**
        **添加命令的方式可能会在后续版本更改**

        Add all Command methods here.
        在这里添加所有命令方法
        
        Command methods can only take one parameter, 
        which is an Godot.Collections.Array and the method cannot have overload.
        命令方法只能有一个参数，该参数必须是 Godot.Collections.Array类型，并且该方法不能重载。
        
        The Command method name will be a console command (case sensitive)
        命令方法名称将作为控制台命令（区分大小写）

        You should handle the possible exceptions that may occur in the command method.
        你应该在命令方法中处理可能出现的异常。

        Command methods cannot be added by inheriting from this class        
        命令方法不能通过继承该类添加
    */
    private void Example(Godot.Collections.Array args)
    {
        Console.GameConsole.PrintNoFormattedMessage("Example command executed with arguments:", args);
    }
}