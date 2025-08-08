using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public static class EventEditorFactory
{
    private static readonly PackedScene _editorScene = ResourceLoader.Load<PackedScene>("res://addons/RuntimeConsole/ObjectInspectorWindow/MemberEditor/InvocableMemberEditor/EventEditor/EventEditor.tscn");
    public static EventEditor Create(
        object target,
        EventInfo eventInfo,
        Godot.Collections.Array<Godot.Collections.Dictionary> connectList = null)
    {
        var editor = _editorScene.Instantiate<EventEditor>();

        string eventName = eventInfo.Name;
        Type delegateType = eventInfo.EventHandlerType;
        var invokeMethod = delegateType.GetMethod("Invoke");
        var parameters = invokeMethod.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        // 构造形如 EventName(int foo, string bar)
        string signature = $"{eventName}({string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))})";
        editor.SetMemberInfo(eventName, signature, parameterTypes);

        var listeners = new Dictionary<string, (object, object)>();

        // 获取 Godot Connect 方式的连接
        foreach (var (callable, original) in GetSignalCallablesExcludingMiddleman(target, eventName, connectList))
        {
            string key;

            if (callable.Delegate is Delegate d)
            {
                var targetObj = d.Target;
                key = FormatListenerKey(targetObj, d.Method);

                // 类型转换
                if (eventInfo.EventHandlerType != d.GetType())
                {
                    try
                    {
                        d = Delegate.CreateDelegate(eventInfo.EventHandlerType, d.Target, d.Method);
                        listeners[key] = (target, d);
                    }
                    catch
                    {
                        // 转换失败，用 Callable，交给 Disconnect 处理
                        listeners[key] = (callable.Target, callable);
                    }
                }
                else
                {
                    listeners[key] = (target, d);
                }
            }
            else if (callable.Target != null && callable.Method != null)
            {
                key = $"{callable.Target.GetType().Name}::{callable.Method}";
                listeners[key] = (callable.Target, callable);
            }
            else
            {
                // 内部绑定（如 NativeClass::method_name）原样保留
                key = original;
                listeners[key] = (null, null);
            }
        }

        // 获取通过 C# += 注册的监听者（通过 backing 字段）
        var multicast = GetMulticastDelegate(target, eventInfo);
        if (multicast != null)
        {
            foreach (var handler in multicast.GetInvocationList())
            {
                string key = FormatListenerKey(handler.Target, handler.Method);
                listeners[key] = (target, handler);
            }
        }

        editor.SetListeners(listeners);
        return editor;
    }

    public static EventEditor Create(GodotObject gdObj, Godot.Collections.Dictionary signalInfo, Godot.Collections.Array<Godot.Collections.Dictionary> connectList)
    {
        var editor = _editorScene.Instantiate<EventEditor>();
        /* 
        { 
            "name": "test_signal", 
            "args": [
                { 
                    "name": "a", 
                    "class_name": &"", 
                    "type": 0, 
                    "hint": 0, 
                    "hint_string": "", 
                    "usage": 131072 
                }, 
                { 
                    "name": "b", 
                    "class_name": &"", 
                    "type": 0, 
                    "hint": 0, 
                    "hint_string": "", 
                    "usage": 131072 
                },
                { 
                    "name": "c", 
                    "class_name": &"", 
                    "type": 0, 
                    "hint": 0, 
                    "hint_string": "", 
                    "usage": 131072 
                }
            ], 
            "default_args": [], 
            "flags": 1, 
            "id": 0, 
            "return": { "name": "", "class_name": &"", "type": 0, "hint": 0, "hint_string": "", "usage": 6 } }
        */
        var name = signalInfo["name"].AsString();
        Dictionary<string, Type> argsType = [];

        var args = signalInfo["args"].AsGodotArray<Godot.Collections.Dictionary>();
        foreach (var arg in args)
        {
            argsType.Add(arg["name"].AsString(), GDScriptUtility.GetPropertyNativeType(arg));
        }
        string signature = $"{name}({string.Join(", ", argsType.Select(p => $"{p.Value.Name} {p.Key}"))})";
        editor.SetMemberInfo(name, signature, argsType.Values.ToArray());

        var listeners = new Dictionary<string, (object, object)>();
        foreach (var (callable, original) in GetSignalCallablesExcludingMiddleman(gdObj, name, connectList))
        {
            string key;

           if (callable.Target != null && callable.Method != null)
            {
                key = $"{callable.Target.GetType().Name}::{callable.Method}";
                listeners[key] = (callable.Target, callable);
            }
            else
            {
                // 内部绑定（如 NativeClass::method_name）原样保留
                key = original;
                listeners[key] = (null, null);
            }
        }

        editor.SetListeners(listeners);
        return editor;
    }

    private static string FormatListenerKey(object target, MethodInfo method)
        => $"{target?.GetType().Name ?? "Static"}::{method?.Name ?? "Unknown"}";

    private static Delegate GetMulticastDelegate(object target, EventInfo eventInfo)
    {
        var type = target.GetType();

        // 首先尝试标准字段名（与事件名相同）
        var field = type.GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        // 如果没找到，尝试通过特性指定的字段名
        if (field == null)
        {
            var attr = eventInfo.GetCustomAttribute<EventFieldNameAttribute>();
            if (attr != null)
            {
                field = type.GetField(attr.FieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            }
        }

        // 如果还没找到，fallback 到 Godot 的 backing_
        if (field == null)
        {
            var fallbackName = $"backing_{eventInfo.Name}";
            field = type.GetField(fallbackName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        return field?.GetValue(target) as Delegate;
    }


    private static List<(Callable callable, string originalString)> GetSignalCallablesExcludingMiddleman(
        object target,
        string eventName,
        Godot.Collections.Array<Godot.Collections.Dictionary> connectList)
    {
        var result = new List<(Callable, string)>();

        if (connectList is not { Count: > 0 } || target is not GodotObject)
            return result;

        foreach (var connect in connectList)
        {
            var callable = connect["callable"].AsCallable();

            // 用于显示内部连接原始字符串形式，转换为Callable后将丢失原始形式，不可读
            var originalString = connect["callable"].ToString();

            // 跳过 EventSignalMiddleman，这个条目是用于转发C#中用户定义信号使用事件的 +=/-= 连接的回调
            if (callable.Method == eventName)
                continue;

            result.Add((callable, originalString));
        }

        return result;
    }
}

/// <summary>
/// 指示在创建事件编辑器时，应从哪个委托类型字段获取事件的监听者列表。<br/>
/// 适用于事件自定义了 add/remove 的封装情况。<br/>
/// <b>默认实现的事件（未显式定义 add/remove 访问器）无需标记此特性，系统将自动尝试查找默认字段名或 Godot 自动生成的 backing 字段。</b>
/// </summary>
[AttributeUsage(AttributeTargets.Event, AllowMultiple = false)]
public class EventFieldNameAttribute : Attribute
{
    /// <summary>
    /// 事件委托实例所存储的字段名称。<br/>
    /// <b>该字段必须为委托类型</b>
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// 指示在创建事件编辑器时，应从哪个委托类型字段获取事件的监听者列表。<br/>
    /// 适用于事件自定义了 add/remove 的封装情况。<br/>
    /// <b>默认实现的事件（未显式定义 add/remove 访问器）无需标记此特性，系统将自动尝试查找默认字段名或 Godot 自动生成的 backing 字段。</b>
    /// </summary>
    /// <param name="fieldName">
    /// 事件委托实例所存储的字段名称。<br/>
    /// <b>该字段必须为委托类型</b>
    /// </param>
    public EventFieldNameAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}
