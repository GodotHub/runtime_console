using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RuntimeConsole;

public class GDScriptMethodProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] context)
    {
        if (obj is not GodotObject gdObj || gdObj.GetScript().Obj is not GDScript gdScript)
            yield break;

        var list = gdScript.GetScriptMethodList();
        GD.Print(list);
        foreach (var method in list)
        {
            var editor = MethodEditorFactory.Create(method);
            editor.Invoke += args =>
            {
                try
                {
                    var argArray = new List<Variant>();

                    if (args != null)
                    {
                        foreach (var arg in args)
                        {
                            argArray.Add(VariantUtility.Create(arg));
                        }
                    }

                    var result = gdObj.Callv(editor.MemberName, new(argArray));

                    if (editor.PinReturnValue && result.Obj != null)
                    {
                        Clipboard.Instance.AddEntry(result);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to invoke GDScript method '{editor.MemberName}': {ex.Message}");
                }
            };

            yield return editor;
        }
    }
}