using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Godot;

namespace RuntimeConsole;

public class EventProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] _)
    {
        var objType = obj.GetType();
        var events = objType.GetEvents();
        Godot.Collections.Array<Godot.Collections.Dictionary> signalList = null;
        GodotObject gdObj = obj as GodotObject;
        if (gdObj != null)
        {
            signalList = gdObj.GetSignalList();            
        }
        foreach (var eventInfo in events)
        {
            bool isSignal = false;
            Godot.Collections.Array<Godot.Collections.Dictionary> connectList = null;
            var signalName = eventInfo.Name;
            if (gdObj != null && signalList != null)
            {
                var matchingSignal = signalList.FirstOrDefault(dict => 
                    dict["name"].ToString().ToPascalCase().Contains(eventInfo.Name));
                if (matchingSignal != null)
                {
                    signalName = matchingSignal["name"].ToString();
                    connectList = gdObj.GetSignalConnectionList(signalName);
                    isSignal = true;
                }
            }
            var editor = EventEditorFactory.Create(obj, eventInfo, connectList);
            editor.Invoke += (args) =>
            {
                try
                {
                    var eventName = eventInfo.Name;
                    var attr = eventInfo.GetCustomAttribute<EventFieldNameAttribute>();
                    if (obj is GodotObject && isSignal)
                    {
                        eventName = $"backing_{eventName}";
                    }
                    else if (attr != null)
                    {
                        eventName = attr.FieldName;
                    }
                    // 获取事件对应的委托字段
                    var fieldInfo = objType.GetField(eventName,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                    if (fieldInfo != null)
                    {
                        var handler = fieldInfo.GetValue(obj) as Delegate;
                        handler?.DynamicInvoke(args);
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error invoking event {eventInfo.Name}: {e.Message}");
                }
            };
            editor.ListenerRemoved += (listener, eventHandler) =>
            {                
                try
                {
                    if (listener != null && eventHandler != null)
                    {
                        if (eventHandler is Delegate handler)
                        {
                            eventInfo.RemoveMethod.Invoke(listener, [handler]);
                        }
                        else if ((eventHandler is Callable callable) && (listener is GodotObject gdObj) && isSignal)
                        {
                            gdObj.Disconnect(signalName, callable);
                        }
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to remove event：{eventInfo.Name} event handler: '{eventHandler}' from listener: '{listener}': {e.Message}");
                }
            };
            yield return editor;
        }
    }
}