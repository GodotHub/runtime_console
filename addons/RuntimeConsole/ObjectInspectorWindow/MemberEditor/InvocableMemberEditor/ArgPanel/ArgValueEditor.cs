using Godot;
using System;

namespace RuntimeConsole;

public partial class ArgValueEditor : HBoxContainer
{
    private Label _argTypeLabel;
    private LineEdit _argValueEdit;
    private Type _argType;

    public bool IsGenericArg
    {
        get => _isGenericArg;
        set
        {
            _isGenericArg = value;
            if (_isGenericArg)
            {
                _argTypeLabel.Text = "Generic Type: ";
            }
        }
    }

    private bool _isGenericArg;

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
            GD.PrintErr(ex.ToString());
            return null;
        }
    }

    private object ParseInput(string input)
    {
        if (input.StartsWith('#'))
        {
            if (int.TryParse(input.AsSpan(1), out var index))
            {
                if (Clipboard.Instance.TryGetValue(index, out var value))
                {
                    if (value.GetType() == _argType || _argType == typeof(Variant) || _argType.IsAssignableFrom(value.GetType()))
                    {
                        return value;
                    }
                    
                    if (IsGenericArg && value is Type && _argType == typeof(Type))
                    {
                        return value;
                    }
                }
    
            }
        }
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Failed parse input! Input is empty!");
        
        // 使用 Convert.ChangeType 进行通用类型转换
        try
        {
            
            if (_argType.IsEnum)
            {
                // 如果是数字，则使用 Enum.ToObject 转换
                if (long.TryParse(input, out var numericValue))
                {
                    return Enum.ToObject(_argType, numericValue);
                }
                // 否则按名称解析
                else
                {
                    return Enum.Parse(_argType, input, true);
                }
            }
            return Convert.ChangeType(input, _argType);
            
        }
        catch (Exception)
        {
            // C# 转换失败，尝试解析为Godot数据类型
            try
            {
                // 特殊处理Godot类型
                if (_argType == typeof(string) || _argType == typeof(Type))
                {
                    return input;
                }
                else if (_argType == typeof(StringName))
                {
                    return new StringName(input);
                }
                else if (_argType == typeof(NodePath))
                {
                    return new NodePath(input);
                }
                else
                {
                    var parseValue = GD.StrToVar(input);
                    if (parseValue.Obj != null && (parseValue.Obj.GetType() == _argType || _argType == typeof(Variant)))
                    {
                        return _argType == typeof(Variant) ? parseValue : parseValue.Obj;
                    }
                }
            }
            catch
            {
                // 忽略异常，继续抛出原始异常
            }

            throw;
        }

    }
}
