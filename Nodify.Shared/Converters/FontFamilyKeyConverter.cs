using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Nodify
{
    /// <summary>
    /// 将字体资源Key字符串转换为FontFamily对象。
    /// 例如 "FontAwesome" → 查找 Application.Current 中的 FontAwesome 资源。
    /// </summary>
    public class FontFamilyKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key && !string.IsNullOrEmpty(key))
            {
                if (Application.Current.TryFindResource(key) is FontFamily fontFamily)
                    return fontFamily;
            }
            // 默认返回 FontAwesome
            if (Application.Current.TryFindResource("FontAwesome") is FontFamily defaultFont)
                return defaultFont;
            return new FontFamily("Segoe UI");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
