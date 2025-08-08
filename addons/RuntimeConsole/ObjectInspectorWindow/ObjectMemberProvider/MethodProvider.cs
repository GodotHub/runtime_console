using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RuntimeConsole;

public partial class MethodProvider : IObjectMemberProvider
{    
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        var methods = obj.GetType().GetMethods();
        
        foreach (var method in methods)
        {
            var editor = MethodEditorFactory.Create(method);
            editor.Invoke += (args) =>
            {
                try
                {
                    MethodInfo methodToInvoke = method;
                    
                    // 如果是泛型方法，需要构造具体类型的方法
                    if (method.IsGenericMethodDefinition && editor.GenericArgsCount > 0)
                    {
                        // 检查args是否有效
                        if (args == null || args.Length < editor.GenericArgsCount)
                        {
                            GD.PrintErr("Invalid arguments for generic method invocation");
                            return;
                        }
                        
                        // 前几个参数是泛型类型参数
                        var genericTypes = new Type[editor.GenericArgsCount];
                        for (int i = 0; i < editor.GenericArgsCount; i++)
                        {
                            var typeArg = args[i];
                            
                            // 如果参数已经是Type类型，直接使用
                            if (typeArg is Type directType)
                            {
                                genericTypes[i] = directType;
                                continue;
                            }
                            
                            // 否则将参数作为字符串处理
                            var typeString = typeArg?.ToString();
                            if (string.IsNullOrEmpty(typeString))
                            {
                                GD.PrintErr("Generic type parameter cannot be null or empty");
                                return;
                            }
                            
                            // 查找类型
                            genericTypes[i] = FindType(typeString);
                            if (genericTypes[i] == null)
                            {
                                GD.PrintErr($"Type '{typeString}' not found");
                                return;
                            }
                        }
                        
                        // 构造泛型方法
                        methodToInvoke = method.MakeGenericMethod(genericTypes);
                    }
                    
                    // 执行方法调用
                    object[] methodArgs;
                    if (args == null)
                    {
                        methodArgs = new object[0];
                    }
                    else
                    {
                        methodArgs = new object[args.Length - editor.GenericArgsCount];
                        if (methodArgs.Length > 0)
                        {
                            Array.Copy(args, editor.GenericArgsCount, methodArgs, 0, methodArgs.Length);
                        }
                    }
                    
                    var result = methodToInvoke.Invoke(obj, methodArgs);

                    if (editor.PinReturnValue && result != null)
                    {
                        Clipboard.Instance.AddEntry(result);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to invoke method '{method.Name}': {ex.Message}");
                }
            };

            yield return editor;
        }
    }
    
    private Type FindType(string typeName)
    {
        // 先尝试直接通过Type.GetType获取
        var type = Type.GetType(typeName);
        if (type != null)
            return type;
            
        // 在当前程序集中查找
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        
        return null;
    }
}