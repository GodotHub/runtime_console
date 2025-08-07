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

        foreach (var key in keys)
        {
            // 获取当前值
            object value = indexer.GetValue(genericDict, new[] { key });

            // 创建编辑器
            PropertyEditorBase valueEditor = PropertyEditorFactory.Create(valueType);

            if (valueEditor is IExpendObjectRequester requester && context != null)
            {
                requester.SetContext([.. context, "element"]);
            }

            valueEditor.SetMemberInfo($"[{key}]", valueType, value, MemberEditorType.Property);

            // 设置值变更处理
            var currentKey = key;
            valueEditor.ValueChanged += newValue =>
            {
                try
                {
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

        foreach (var item in collection)
        {
            PropertyEditorBase editor = PropertyEditorFactory.Create(item?.GetType() ?? typeof(object));
            
            if (editor is IExpendObjectRequester requester && context != null)
            {
                requester.SetContext([.. context, "element"]);// 传递上下文，追加"element"信息，指示
            }

            editor.SetMemberInfo($"[{idx}]", item?.GetType() ?? typeof(object), item, MemberEditorType.Property);

            // 为可写集合添加值变更处理
            if (!isReadOnly && hasIndexer)
            {
                var currentIndex = idx;
                editor.ValueChanged += newValue =>
                {
                    try
                    {
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
