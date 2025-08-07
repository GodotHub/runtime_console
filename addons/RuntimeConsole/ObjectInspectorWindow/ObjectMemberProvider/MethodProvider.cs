using Godot;
using System;
using System.Collections.Generic;

namespace RuntimeConsole;

public partial class MethodProvider : IObjectMemberProvider
{    
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] context)
    {
        var methods = obj.GetType().GetMethods();
        
        foreach (var method in methods)
        {
            var editor = MethodEditorFactory.Create(method);
            editor.Invoke += (args) =>
            {
                try
                {
                    var result = method.Invoke(obj, args);

                    if (editor.PinReturnValue && result != null)
                    {
                        Clipboard.Instance.AddEntry(result);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr(ex.ToString());
                }
            };

            yield return editor;

        }

        
    }

}
