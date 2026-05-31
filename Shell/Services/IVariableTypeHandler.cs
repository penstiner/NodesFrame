using System.Windows;
using Shell.Models;

namespace Shell.Services
{
    /// <summary>
    /// 变量类型的处理器：负责字符串↔值转换、编辑器/显示器创建。
    /// 新增变量类型只需实现此接口并注册到 VariableTypeRegistry。
    /// </summary>
    public interface IVariableTypeHandler
    {
        /// <summary>类型标识名（如 "Boolean"、"Axis"、"ImageData"）。</summary>
        string TypeName { get; }

        /// <summary>字符串 → VariantValue 解析（解析失败返回 fallback）。</summary>
        VariantValue Parse(string text, VariantValue fallback);

        /// <summary>VariantValue → 显示文本。</summary>
        string GetDisplayText(VariantValue value);

        /// <summary>创建编辑器控件（用于对话框中编辑值）。</summary>
        FrameworkElement CreateEditor(Variable variable);

        /// <summary>创建只读显示控件（用于面板中展示值）。</summary>
        FrameworkElement CreateDisplay(Variable variable);
    }
}
