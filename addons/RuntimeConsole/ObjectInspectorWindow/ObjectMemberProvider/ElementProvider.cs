using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeConsole;

public class ElementProvider : IObjectMemberProvider
{
    public IEnumerable<IMemberEditor> Populate(object obj)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(obj.GetType()))
            throw new ArgumentException("obj is not IEnumerable");

        var collection = (IEnumerable)obj;

        var idx = 0;
        foreach (var item in collection)
        {
            var type = item.GetType();
            var editor = PropertyEditorFactory.Create(type);

            editor.SetMemberInfo($"[{idx}]", type, item, MemberEditorType.Property);

            if (collection is IList list)
            {
                var currentIndex = idx;
                editor.ValueChanged += (value) =>
                {
                    list[currentIndex] = value;
                };
            }

            idx++;

            yield return editor;
        }
    }
}
