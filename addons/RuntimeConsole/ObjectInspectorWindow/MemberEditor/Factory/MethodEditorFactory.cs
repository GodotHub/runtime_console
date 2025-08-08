using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public static class MethodEditorFactory
{
    private static readonly PackedScene _methodEditorScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/InvocableMemberEditor/MethodEditor/MethodEditor.tscn");
    public static MethodEditor Create(MethodInfo methodInfo)
    {
        var editor = _methodEditorScene.Instantiate<MethodEditor>();
        var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
        var genericArgsCount = methodInfo.IsGenericMethodDefinition ? methodInfo.GetGenericArguments().Length : 0;
        editor.SetMemberInfo(methodInfo.Name, methodInfo.ToString(), parameterTypes, genericArgsCount);
        return editor;
    }

    public static MethodEditor Create(Godot.Collections.Dictionary methodInfo)
    {
        var editor = _methodEditorScene.Instantiate<MethodEditor>();
        var name = methodInfo["name"].AsString();
        List<Type> argsType = [];

        var args = methodInfo["args"].AsGodotArray<Godot.Collections.Dictionary>();
        foreach (var arg in args)
        {
            argsType.Add(GDScriptUtility.GetPropertyNativeType(arg));
        }
        var returnValue = methodInfo["return"].AsGodotDictionary();
        var returnType = returnValue["type"].As<Variant.Type>();
        var returnClassName = returnValue["class_name"].AsString();
        var returnValueFlag = returnValue["usage"].AsInt64();
        // 构建方法签名
        var signature = BuildMethodSignature(name, argsType.ToArray(), returnType, returnClassName, returnValueFlag);
        
        int genericArgsCount = 0;
        
        editor.SetMemberInfo(name, signature, argsType.ToArray(), genericArgsCount);
        return editor;
    }

    private static string BuildMethodSignature(string name, Type[] args, Variant.Type returnType, string returnClassName, long returnValueFlag)
    {
        var argsString = string.Join(", ", args.Select(a => a.Name));
        string returnTypeName;

        if (returnType == Variant.Type.Object && !string.IsNullOrEmpty(returnClassName))
        {
            returnTypeName = returnClassName;
        }
        else
        {
            var nativeType = VariantUtility.GetNativeType(returnType);
            if ((returnValueFlag & (long)PropertyUsageFlags.NilIsVariant) != 0 && nativeType == null)
            {
                returnTypeName = "Variant";
            }
            else
            {
                returnTypeName = nativeType?.Name ?? "Void";

            }
        }
        
        return $"{returnTypeName} {name}({argsString})";
    }
}