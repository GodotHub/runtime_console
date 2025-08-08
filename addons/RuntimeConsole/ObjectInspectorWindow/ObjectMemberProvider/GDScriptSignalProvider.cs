using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

public class GDScriptSignalProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        if (obj is not GodotObject gdObj || gdObj?.GetScript().Obj is not GDScript gdScript)
            yield break;

        var signalList = gdScript.GetScriptSignalList();

        foreach (var signal in signalList)
        {
            var signalName = signal["name"].AsString();
            var connectList = gdObj.GetSignalConnectionList(signalName);

            var editor = EventEditorFactory.Create(gdObj, signal, connectList);

            editor.Invoke += args =>
            {
                try
                {
                    var varArgs = args.Select(x => VariantUtility.Create(x)).ToArray();
                    gdObj.EmitSignal(signalName, varArgs);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error emitting signal {signalName}: {e.Message}");
                }
            };

            editor.ListenerRemoved += (_, eventHandler) =>
            {
                try
                {
                    if (eventHandler != null && eventHandler is Callable callable)
                    {
                        gdObj.Disconnect(signalName, callable);
                    }
                    else
                    {
                        GD.PrintErr($"Failed to remove listener for signal {signalName} on {gdObj.GetType().Name}.");
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error disconnecting signal {signalName}: {e.Message}");
                }
            };

            yield return editor;
        }
        

    }
 
}