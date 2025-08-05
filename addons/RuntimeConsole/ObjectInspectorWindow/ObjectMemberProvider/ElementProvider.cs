using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace RuntimeConsole;

public class ElementProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj)
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

        // 处理其他集合类型（List、IList等）
        return PopulateGenericCollection(collection, collectionType);
    }

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

    private IEnumerable<IMemberEditor> PopulateGenericCollection(IEnumerable collection, Type collectionType)
    {
        int idx = 0;
        bool isReadOnly = IsCollectionReadOnly(collection);
        
        // 尝试获取索引器（对于支持索引的集合）
        PropertyInfo indexer = collectionType.GetProperty("Item");
        bool hasIndexer = indexer != null && indexer.CanRead && indexer.GetIndexParameters().Length == 1;
        
        foreach (var item in collection)
        {
            var editor = PropertyEditorFactory.Create(item?.GetType() ?? typeof(object));
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
                        indexer.SetValue(collection, newValue, new object[] { currentIndex });
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

    private bool IsArrayOrList(Type type)
    {
        return type.IsArray || 
            typeof(IList).IsAssignableFrom(type) || 
            type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
    }
}
