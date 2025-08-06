using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public class ElementProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj, params object[] context)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(obj.GetType()))
            throw new ArgumentException("obj is not IEnumerable");

        var collection = (IEnumerable)obj;
        var collectionType = obj.GetType();

        // 处理多维数组
        if (obj is Array array && array.Rank > 1)
        {
            return PopulateMultidimensionalArray(array);
        }

        // 处理普通数组（一维）和锯齿数组
        if (collectionType.IsArray)
        {
            return PopulateArray((Array)obj);
        }

        // 非泛型字典/C# 原生字典
        if (obj is IDictionary dic)
        {
            return PopulateTable(dic);
        }

        // 处理字典/Godot字典
        if (IsGenericDictionary(collectionType))
        {
            return PopulateGenericDictionary(obj, context);
        }

        // 处理List、IList
        if (IsList(collectionType))
        {            
            return PopulateList(collection, collectionType, context);
        }

        return PopulateCollection(collection);
    }

    private bool IsGenericDictionary(Type type)
    {        
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));        
    }

    private bool IsList(Type type)
    {
        return type.IsAssignableTo(typeof(IList)) || type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
    }

    // 处理其他不可编辑的集合，Stack，Queue，HashSet
    private IEnumerable<IMemberEditor> PopulateCollection(IEnumerable collection)
    {
        int idx = 0;
        foreach (var item in collection)
        {
            var editor = PropertyEditorFactory.Create(item?.GetType() ?? typeof(object));
            editor.SetMemberInfo($"[{idx}]", item?.GetType() ?? typeof(object), item, MemberEditorType.Property);
            editor.SetEditable(false);
            yield return editor;

            idx++;
        }
    }

    // 处理非泛型字典
    private IEnumerable<IMemberEditor> PopulateTable(IDictionary dict)
    {
        bool isReadOnly = dict.IsReadOnly;

        foreach (DictionaryEntry entry in dict)
        {
            object key = entry.Key;
            object value = entry.Value;

            var valueEditor = PropertyEditorFactory.Create(value?.GetType() ?? typeof(object));
            valueEditor.SetMemberInfo($"[{key}]", value?.GetType() ?? typeof(object), value, MemberEditorType.Property);

            if (!isReadOnly)
            {
                var currentKey = key; // 闭包捕获当前键
                valueEditor.ValueChanged += newValue =>
                {
                    try
                    {
                        dict[currentKey] = newValue;
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Failed to update dictionary key [{currentKey}]: {ex.Message}");
                    }
                };
            }

            yield return valueEditor;
        }
    }

    // 处理泛型字典、Godot字典
    private IEnumerable<IMemberEditor> PopulateGenericDictionary(object genericDict, object[] context)
    {
        var dictType = genericDict.GetType();

        Type keyType, valueType;
        if (dictType.GetGenericArguments().Length == 2)
        {
            keyType = dictType.GetGenericArguments()[0];
            valueType = dictType.GetGenericArguments()[1];
        }
        else
        {
            // 泛型参数少于2，则认作Godot字典
            keyType = typeof(Variant);
            valueType = typeof(Variant);
        }

        // 获取Keys属性
        var keysProp = dictType.GetProperty("Keys");
        var keys = (IEnumerable)keysProp.GetValue(genericDict);

        // 获取索引器
        var indexer = dictType.GetProperty("Item");

        bool isGodotEnumDict = false;
        Dictionary<string, int> godotEnumDict = null;
        if (context != null && context.Length > 0)
        {
            if (context[0] is Godot.Collections.Dictionary gdsPropInfo)
            {
                var type = gdsPropInfo["type"].As<Variant.Type>();
                var hint = gdsPropInfo["hint"].As<PropertyHint>();
                var hintString = gdsPropInfo["hint_string"].AsString();

                // 处理类型化的枚举字典，这里只处理字典值为枚举的情况。
                // 由于 GDScript 枚举在导出到 C# 时，
                // C# 端只会拿到整数值，无法还原用户在 GDScript 中实际书写的“具体枚举名”。
                // 若强行反查枚举值所对应的所有枚举名，会在 UI 上展示出多个名称（如 Foo / Bar / Baz），使用户产生混淆。
                // 因此，当字典的键为枚举时，直接显示其整数常量值，不做枚举名解析。
                // 如果存在相同值的枚举，则无法保证解析后是否对应正确的枚举名，只会匹配第一个为该常量值的枚举名！
                if (type == Variant.Type.Dictionary && hint == PropertyHint.TypeString && IsEnumDict(hintString))
                {
                    godotEnumDict = GDScriptUtility.GetGDScriptDictionaryValueEnum(hintString);
                    isGodotEnumDict = true;
                }

            }
        }
        foreach (var key in keys)
        {
            // 获取当前值
            object value = indexer.GetValue(genericDict, new[] { key });

            // 创建编辑器
            PropertyEditorBase valueEditor = null;
            object memberValue = value;
            if (isGodotEnumDict && godotEnumDict != null && godotEnumDict.Count > 0)
            {
                valueEditor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Enum);
                memberValue = (godotEnumDict, (Variant)value);
            }
            else
            {
                valueEditor = PropertyEditorFactory.Create(valueType);
            }
            valueEditor.SetMemberInfo($"[{key}]", valueType, memberValue, MemberEditorType.Property);

            // 设置值变更处理
            var currentKey = key;
            valueEditor.ValueChanged += newValue =>
            {
                try
                {
                    if (isGodotEnumDict)
                        newValue = VariantUtility.Create(newValue);
                    // 通过键更新值
                    indexer.SetValue(genericDict, newValue, new[] { currentKey });
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to update dictionary key [{currentKey}]: {ex.Message}");
                }
            };

            yield return valueEditor;
        }
    }
  
    // 处理数组
    private IEnumerable<IMemberEditor> PopulateArray(Array array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            var value = array.GetValue(i);
            var editor = PropertyEditorFactory.Create(value?.GetType() ?? typeof(object));
            editor.SetMemberInfo($"[{i}]", value?.GetType() ?? typeof(object), value, MemberEditorType.Property);

            // 数组总是可写（除非是只读数组）
            if (!array.IsReadOnly)
            {
                var currentIndex = i;
                editor.ValueChanged += newValue =>
                {
                    try
                    {
                        array.SetValue(newValue, currentIndex);
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Failed to update array element at index {currentIndex}: {ex.Message}");
                    }
                };
            }

            yield return editor;
        }
    }

    // 处理多维数组
    private IEnumerable<IMemberEditor> PopulateMultidimensionalArray(Array array)
    {
        // 创建索引数组用于遍历所有维度
        var indices = new int[array.Rank];

        // 设置初始索引（每个维度从0开始）
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = array.GetLowerBound(i);
        }

        do
        {
            // 获取当前索引对应的值
            var value = array.GetValue(indices);

            // 创建索引字符串表示（如 [1,2,3]）
            var indexStr = $"[{string.Join(",", indices)}]";

            // 创建编辑器
            var editor = PropertyEditorFactory.Create(value.GetType());
            editor.SetMemberInfo(indexStr, value.GetType(), value, MemberEditorType.Property);

            // 设置值改变事件（仅当数组可写时）
            if (!array.IsReadOnly)
            {
                var currentIndices = (int[])indices.Clone();
                editor.ValueChanged += newValue =>
                {
                    try
                    {
                        array.SetValue(newValue, currentIndices);
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Failed to update array element at {indexStr}: {ex.Message}");
                    }
                };
            }

            yield return editor;

        } while (IncrementIndices(array, indices)); // 移动到下一个索引位置
    }

    // 处理列表、Godot数组
    private IEnumerable<IMemberEditor> PopulateList(IEnumerable collection, Type collectionType, object[] context)
    {
        int idx = 0;
        bool isReadOnly = IsCollectionReadOnly(collection);

        // 尝试获取索引器（对于支持索引的集合）
        PropertyInfo indexer = collectionType.GetProperty("Item");
        bool hasIndexer = indexer != null && indexer.CanRead && indexer.GetIndexParameters().Length == 1;
        bool isGodotEnumArray = false;
        bool isGodotFlagsArray = false;

        Dictionary<string, int> godotEnumDict = null;
        Dictionary<string, int> godotFlagsDict = null;
        
        // 只有从GDScript端获取到的枚举集合，才会进入这个分支，这不会影响到C#的GodotArray枚举集合
        if (context != null && context.Length > 0)
        {
            if (context[0] is Godot.Collections.Dictionary gdsPropInfo)
            {
                var type = gdsPropInfo["type"].As<Variant.Type>();
                var hint = gdsPropInfo["hint"].As<PropertyHint>();
                var hintString = gdsPropInfo["hint_string"].AsString();

                // 如果存在相同值的枚举，则无法保证解析后是否对应正确的枚举名，只会匹配第一个为该常量值的枚举名！
                // 处理类型化的枚举数组
                if (type == Variant.Type.Array && hint == PropertyHint.TypeString && IsEnumArray(hintString))
                {
                    // $"{Variant.Type.Int:D}/{PropertyHint.Enum:D}:Zero,One,Three:3,Six:6"
                    // 2/2:Zero,One,Three:3,Six:6
                    godotEnumDict = GDScriptUtility.GetGDScriptEnum(hintString.Substring(4));
                    isGodotEnumArray = true;
                }

                // 处理类型化标志位数组
                if (type == Variant.Type.Array && hint == PropertyHint.TypeString && IsFlagArray(hintString))
                {
                    // $"{Variant.Type.Int:D}/{PropertyHint.Flags:D}:Zero,One,Three:2,Six:6"
                    // 2/6:Zero,One,Three:2,Six:6
                    godotFlagsDict = GDScriptUtility.GetGDScriptFlags(hintString.Substring(4));
                    isGodotFlagsArray = true;

                }
            }
        }

        foreach (var item in collection)
        {
            PropertyEditorBase editor = null; 
            object memberValue = item;
            
            if (isGodotEnumArray && godotEnumDict != null && godotEnumDict.Count > 0)
            {
                memberValue = (godotEnumDict, (Variant)item);
                editor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Enum);
            }
            else if (isGodotFlagsArray && godotFlagsDict != null && godotFlagsDict.Count > 0)
            {
                memberValue = (godotFlagsDict, (Variant)item);
                editor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Flags);
            }
            else
            {
                editor = PropertyEditorFactory.Create(item?.GetType() ?? typeof(object));
            }

            editor.SetMemberInfo($"[{idx}]", item?.GetType() ?? typeof(object), memberValue, MemberEditorType.Property);

            // 为可写集合添加值变更处理
            if (!isReadOnly && hasIndexer)
            {
                var currentIndex = idx;
                editor.ValueChanged += newValue =>
                {
                    try
                    {
                        if (isGodotEnumArray || isGodotFlagsArray)
                            newValue = VariantUtility.Create(newValue);

                        // 使用索引器设置值
                        indexer.SetValue(collection, newValue, [currentIndex]);
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Failed to update collection item at index {currentIndex}: {ex.Message}");
                    }
                };
            }

            idx++;
            yield return editor;
        }
    }

    private bool IsEnumDict(string hintString)
    {
        // 检测形如 "KeyType;2/2:EnumName1,EnumName2:Value1,EnumName2:Value2" 的格式
        // 前缀是 "2/" 表示 Variant.Type.Int (2), 后面是 PropertyHint.Enum (2)
        if (string.IsNullOrEmpty(hintString))
            return false;

        var valueParts = hintString.Split(';');
        if (valueParts.Length != 2)
            return false;

        // 分离类型类型标识和枚举键值
        var parts = valueParts[1].Split(':');
        if (parts.Length < 2)
            return false;

        var typeHintPart = parts[0]; // 这里获取数组类型标识：2/2
        var typeHintTokens = typeHintPart.Split('/'); // 分离字典值类型和枚举hint
        if (typeHintTokens.Length != 2)
            return false;

        // 索引0是元素类型，索引1是枚举hint
        if (int.TryParse(typeHintTokens[0], out int variantType) && int.TryParse(typeHintTokens[1], out int propertyHint))
        {
            return variantType == (int)Variant.Type.Int && propertyHint == (int)PropertyHint.Enum;
        }

        return false;
    }

    private bool IsFlagArray(string hintString)
    {
        // 检测形如 "2/6:EnumName1,EnumName2:Value1,EnumName2:Value2" 的格式
        // 前缀是 "2/" 表示 Variant.Type.Int (2), 后面是 PropertyHint.Flags (6)
        if (string.IsNullOrEmpty(hintString))
            return false;

        // 分离类型类型标识和枚举键值
        var parts = hintString.Split(':');
        if (parts.Length < 2)
            return false;

        var typeHintPart = parts[0]; // 这里获取数组类型标识：2/6
        var typeHintTokens = typeHintPart.Split('/'); // 分离数组元素类型和枚举hint
        if (typeHintTokens.Length != 2)
            return false;

        // 索引0是元素类型，索引1是枚举hint
        if (int.TryParse(typeHintTokens[0], out int variantType) && int.TryParse(typeHintTokens[1], out int propertyHint))
        {
            return variantType == (int)Variant.Type.Int && propertyHint == (int)PropertyHint.Flags;
        }

        return false;
    }

    private bool IsEnumArray(string hintString)
    {
        // 检测形如 "2/2:EnumName1,EnumName2:Value1,EnumName2:Value2" 的格式
        // 前缀是 "2/" 表示 Variant.Type.Int (2), 后面是 PropertyHint.Enum (2)
        if (string.IsNullOrEmpty(hintString))
            return false;

        // 分离类型类型标识和枚举键值
        var parts = hintString.Split(':');
        if (parts.Length < 2)
            return false;

        var typeHintPart = parts[0]; // 这里获取数组类型标识：2/2
        var typeHintTokens = typeHintPart.Split('/'); // 分离数组元素类型和枚举hint
        if (typeHintTokens.Length != 2)
            return false;

        // 索引0是元素类型，索引1是枚举hint
        if (int.TryParse(typeHintTokens[0], out int variantType) && int.TryParse(typeHintTokens[1], out int propertyHint))
        {
            return variantType == (int)Variant.Type.Int && propertyHint == (int)PropertyHint.Enum;
        }

        return false;
    }

    private bool IncrementIndices(Array array, int[] indices)
    {
        // 从最后一个维度开始递增
        for (int dim = array.Rank - 1; dim >= 0; dim--)
        {
            indices[dim]++;

            // 检查当前维度是否超出上限
            if (indices[dim] <= array.GetUpperBound(dim))
                return true;

            // 重置当前维度并进位到上一个维度
            indices[dim] = array.GetLowerBound(dim);
        }

        // 所有维度都已遍历完
        return false;
    }

    private bool IsCollectionReadOnly(object collection)
    {
        // 检查集合是否只读
        if (collection is Array array)
            return array.IsReadOnly;

        var isReadOnlyProp = collection.GetType().GetProperty("IsReadOnly");
        if (isReadOnlyProp != null)
            return (bool)isReadOnlyProp.GetValue(collection);

        return false; // 默认可写
    }

}
