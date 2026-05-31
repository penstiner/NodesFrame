using System;
using System.Globalization;
using System.Windows.Data;

namespace Shell.Converter
{
    /// <summary>
    /// "True"/"False" 字符串 ↔ bool 双向转换（供 CheckBox 绑定 Variable.ValueString）。
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string s && bool.TryParse(s, out var b) && b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? "True" : "False";
        }
    }
}
