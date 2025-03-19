#if TOOLS
using System.Linq;
using Godot;
using static Console;

public partial class ConsoleCommands : Node
{
    public static ConsoleCommands Commands {get; private set;}

    public override void _Ready()
    {
        Commands = this;
    }

/*
    Add all Command methods here.
    
    Command methods can only take one parameter, 
    which is an Godot.Collections.Array and the method cannot have overload.
    
    The Command method name will be a console command (case sensitive)

    You should handle the possible exceptions that may occur in the command method.

    Command methods cannot be added by inheriting from this class
*/

    public void ExampleCommand(Godot.Collections.Array _)
    {
        GameConsole.PrintNoFormattedMessage("Example command!");
    }

    public void ExampleCommand2(Godot.Collections.Array args)
    {
        try
        {
            if (args.Count < 1)
                throw new System.ArgumentException("At least 1 argument required");
                
            int number = (int)args[0]; // It may throw an InvalidCastException.
            GameConsole.PrintNoFormattedMessage($"Square: {number * number}");
        }
        catch (System.Exception e)
        {
            GameConsole.PrintNoFormattedMessage($"Error: {e.Message}");
            GameConsole.PrintNoFormattedMessage("Usage: ExampleCommand2 <integer>");
        }
    }

    public void ClearConsole(Godot.Collections.Array _)
    {
        GameConsole.Clear();
    }

}
#endif