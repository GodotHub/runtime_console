using System;
using Godot;

namespace RuntimeConsole;

public partial class CharPropertyEditor : PropertyEditorBase
{
    private LineEdit _lineEdit;
    private object _value;
    private object _tempValue;

    public override object GetValue()
    {
        return _value;
    }

    public override void SetEditable(bool editable)
    {
        Editable = editable;
        _lineEdit.Editable = editable;
        _editButton.Disabled = !editable;
    }

    protected override void OnSceneInstantiated()
    {
        base.OnSceneInstantiated();

        _lineEdit ??= GetNode<LineEdit>("%ValueEditor");

        _lineEdit.TextChanged += OnTextChanged;
    }

    protected override void OnSubmission()
    {
        if (Editable)
        {
            SetValue(_lineEdit.Text);

            if (!_tempValue.Equals(_value))
            {
                _value = _tempValue;
                NotificationValueChanged();
            }
        }
    }

    protected override void SetValue(object value)
    {
        if (value is not char c)
            return;

        _lineEdit.Text = c.ToString();
        _value = c;
        _tempValue = c;
    }

    private void OnTextChanged(string newText)
    {
        // 判断是否是合法转义
        bool isEscape = IsValidCharEscape(newText);

        if (isEscape)
        {
            // 尝试还原真实字符
            char? parsedChar = TryParseCharEscape(newText);
            if (parsedChar.HasValue)
            {
                _tempValue = parsedChar.Value;
            }
        }
        else if (newText.Length > 1)
        {
            // 非转义、多字符，裁剪并设置
            string trimmed = newText.Substring(0, 1);
            _lineEdit.Text = trimmed;
            _tempValue = trimmed[0];
        }
        else if (newText.Length == 1)
        {
            _tempValue = newText[0];
        }
        else if (string.IsNullOrEmpty(newText))
        {
            _tempValue = default(char);
        }
    }


    /// <summary>
    /// 判断输入是否是合法的字符转义序列，例如 \n、\t、\\、\u1234 等
    /// </summary>
    private bool IsValidCharEscape(string input)
    {
        if (string.IsNullOrEmpty(input) || input[0] != '\\')
            return false;

        // 单字符转义
        if (input == "\\n" || input == "\\t" || input == "\\r" || input == "\\b" ||
            input == "\\f" || input == "\\'" || input == "\\\"" || input == "\\\\" ||
            input == "\\0")
            return true;

        // \uXXXX 格式
        if (input.StartsWith("\\u") && input.Length == 6)
        {
            string hex = input.Substring(2);
            return int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out _);
        }

        // \UXXXXXXXX 格式
        if (input.StartsWith("\\U") && input.Length == 10)
        {
            string hex = input.Substring(2);
            return int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out _);
        }

        return false;
    }
    private char? TryParseCharEscape(string input)
    {
        try
        {
            // 特殊处理 \UXXXXXXXX 格式
            if (input.StartsWith("\\U") && input.Length == 10)
            {
                string hex = input.Substring(2);
                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                {
                    // 在char范围内码点
                    if (codePoint <= 0xFFFF)
                    {
                        return (char)codePoint;
                    }
                    else
                    {
                        // 超出char范围的码点，这里返回null
                        // 因为char只能存储16位值
                        return null;
                    }
                }
            }
            // 特殊处理 \uXXXX 格式
            else if (input.StartsWith("\\u") && input.Length == 6)
            {
                string hex = input.Substring(2);
                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                {
                    return (char)codePoint;
                }
            }
            else
            {
                // 利用 Regex.Unescape 来还原其他转义字符
                string parsed = System.Text.RegularExpressions.Regex.Unescape(input);

                // 有些转义比如 emoji 是两个 UTF-16 code unit（代理对）
                if (parsed.Length == 1)
                    return parsed[0];

                // 若长度为2，说明是代理对
                if (parsed.Length == 2 && char.IsSurrogatePair(parsed[0], parsed[1]))
                {
                    // char只能存储16位，对于代理对只能存储其中一个字符
                    // 这里返回高代理项
                    return parsed[0];
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to parse escape sequence '{input}': {ex.Message}");
        }

        return null;
    }

}
