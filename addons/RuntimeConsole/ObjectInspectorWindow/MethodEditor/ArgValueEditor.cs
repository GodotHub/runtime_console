using Godot;
using System;

namespace RuntimeConsole;

public partial class ArgValueEditor : HBoxContainer
{
    private Label _argTypeLabel;
    private LineEdit _argValueEdit;
    private Type _argType;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            OnSceneInstantiated();
        }

    }

    private void OnSceneInstantiated()
    {
        _argTypeLabel = GetNode<Label>("%ArgType");
        _argValueEdit = GetNode<LineEdit>("%ArgValue");
    }

    public void SetArgInfo(Type argType)
    {
        _argTypeLabel.Text = argType.ToString();
        _argType = argType;
    }

    public object GetValue()
    {
        try
        {
            return ParseInput(_argValueEdit.Text);
        }
        catch (Exception ex)
        {
            _argValueEdit.Text = ex.ToString();
            return null;
        }
    }

    private object ParseInput(string input)
    {
        if (input.StartsWith('#'))
        {
            if (int.TryParse(input.AsSpan(1), out var index) && Clipboard.Instance.TryGetValue(index, out var value) && value.GetType() == _argType)
            {
                return value;
            }
        }

        
        // 使用 Convert.ChangeType 进行通用类型转换
        try
        {
            if (!string.IsNullOrEmpty(input))
            {
                return Convert.ChangeType(input, _argType);
            }
        }
        catch (Exception)
        {
            // C# 转换失败，尝试解析为Godot数据类型
            var parseValue = GD.StrToVar(input);
            if (parseValue.Obj != null)
            {
                return parseValue.Obj;
            }
            
            throw;
        }

        throw new ArgumentException("Failed parse input!");
    }
}
