using System;
using Godot;

namespace RuntimeConsole;

public partial class VariantPropertyEditor : PropertyEditorBase, IExpendObjectRequester
{
    private Variant _value;

    public event RequestCreateNewPanelEventHandler CreateNewPanelRequested;
    private object[] _context;

    // 这里重写父类的初始化逻辑，重写成空实现，因为该编辑器是根据构造时传入的Variant自动分派其他编辑器，不需要场景初始化
    protected override void OnSceneInstantiated()
    {
        return;
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;

        if (GetChildCount() > 0)
        {
            (GetChild(0) as PropertyEditorBase)?.SetEditable(editable);
        }
    }

    // 这里重写为空实现，因为提交修改由该编辑器创建的子编辑器处理
    protected override void OnSubmission()
    {
        return;
    }

    protected override void SetProperty(string name, Type type, object value)
    {
        MemberName = name;
        PropertyType = type;
        SetValue(value);
    }

    protected override void SetValue(object value)
    {
        if (value is not Variant variant)
            return;

        _value = variant;

        PropertyEditorBase editor = null;

        if (_context != null && _context.Length > 0 && _context[0] is Godot.Collections.Dictionary propInfo)
        {
            var hint = propInfo["hint"].As<PropertyHint>();
            var hintString = propInfo["hint_string"].AsString();
            var type = propInfo["type"].As<Variant.Type>();

            /*
            注意：若枚举中存在多个枚举项具有相同的常量值，C# 反查枚举名时无法保证解析出的名称是否为用户实际书写的那个枚举名。
            反查操作只会返回第一个与该值匹配的枚举项名称。
            因此，在处理枚举数组或枚举字典时，若存在值重复的枚举项，将无法确保UI展示的是用户期望的枚举名。
            
            这里只有导出变量才能被识别为枚举，因为这样可以直接通过HintString获取到枚举值和枚举名，而不需要去使用GDScript表达式调用find_key、反射所有Godot枚举来获取枚举值和枚举名
            如果没有导出枚举变量或位标记变量，则直接当作普通的变量处理
            导出枚举也能用在字符串上，但是这里只处理整数的枚举变量，因为这是最常的用法：@export var my_enum = MyEnum.ONE; @export var my_enum: MyEnum
            将字符串以枚举导出得使用 @export_enum("ONE","TWO","THREE") var my_enum = "ONE"
            所以这里是在处理导出整数枚举，以防混淆保持一致性，不处理导出字符串枚举

            处理类型化的枚举字典，这里只处理字典值为枚举的情况。
            由于 GDScript 枚举在导出到 C# 时，
            C# 端只会拿到整数值，无法还原用户在 GDScript 中实际书写的“具体枚举名”。
            若强行反查枚举值所对应的所有枚举名，会在 UI 上展示出多个名称（如 Foo / Bar / Baz），使用户产生混淆。
            因此，当字典的键为枚举时，直接显示其整数常量值，不做枚举名解析。
            如果存在相同值的枚举，则无法保证解析后是否对应正确的枚举名，只会匹配第一个为该常量值的枚举名！
            */
            if (IsGDScriptEnum(hint, type, hintString, _context))
            {
                var enumValues = GDScriptUtility.GetGDScriptEnum(hintString);
                if (enumValues.Count > 0)
                {
                    editor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Enum);
                    editor.SetMemberInfo(MemberName, typeof(Variant), (enumValues, variant), MemberEditorType.Property);
                }
            }
            else if (IsGDScriptFlags(hint, type, hintString, _context))
            {
                var flagValues = GDScriptUtility.GetGDScriptFlags(hintString);
                if (flagValues.Count > 0)
                {
                    editor = PropertyEditorFactory.CreateEnumEditorForGDScript(PropertyHint.Flags);
                    editor.SetMemberInfo(MemberName, typeof(Variant), (flagValues, variant), MemberEditorType.Property);
                }
            }
        }

        if (editor == null)
        {
            // type为空，即Variant没有赋值
            var type = (variant.Obj?.GetType()) ?? typeof(object);
            editor = PropertyEditorFactory.Create(type);
        }
        
        editor.SetEditable(Editable);
        editor.SetMemberInfo(MemberName, typeof(Variant), variant.Obj, MemberType);
        editor.Name = MemberName;
        editor.ValueChanged += OnValueChanged;

        // 转发事件
        if (editor is IExpendObjectRequester requester)
        {
            requester.SetContext(_context); // 传给集合提供者，然后它会再传回来
            requester.CreateNewPanelRequested += OnCreateNewPanelRequested;
        }   
        AddChild(editor);
    }
    private bool IsGDScriptEnum(PropertyHint hint, Variant.Type type, string hintString, object[] context)
    {
        // 顶层导出变量为枚举类型
        if (hint == PropertyHint.Enum && type == Variant.Type.Int)
            return true;

        // 集合中的元素（元素是枚举）
        if (IsCollectionElementContext(context) &&
            hint == PropertyHint.TypeString &&
            ((type == Variant.Type.Array && GDScriptUtility.IsEnumArray(hintString)) ||
            (type == Variant.Type.Dictionary && GDScriptUtility.IsEnumDict(hintString))))
            return true;

        return false;
    }

    private bool IsGDScriptFlags(PropertyHint hint, Variant.Type type, string hintString, object[] context)
    {
        // 顶层导出变量为Flags类型
        if (hint == PropertyHint.Flags && type == Variant.Type.Int)
            return true;

        // 集合中的元素（元素是位标志）
        if (IsCollectionElementContext(context) &&
            hint == PropertyHint.TypeString &&
            type == Variant.Type.Array &&
            GDScriptUtility.IsFlagArray(hintString))
            return true;

        return false;
    }

    private bool IsCollectionElementContext(object[] context)
    {
        return context?.Length > 1 && context[1] is string info && info == "element";
    }

    private void OnValueChanged(object value)
    {
        if (!value.Equals(_value.Obj))
        {            
            _value = VariantUtility.Create(value);
            NotificationValueChanged();
        }
    }

    private void OnCreateNewPanelRequested(PropertyEditorBase editor, object obj, object[] context)
        => CreateNewPanelRequested?.Invoke(editor, obj, context);

    public void OnPanelCreated(ObjectMemberPanel panel)
    {

    }

    public void SetContext(object[] context)
    {
        _context = context;
    }
}