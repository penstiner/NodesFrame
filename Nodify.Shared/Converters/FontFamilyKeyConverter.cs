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
        private static FontFamily? s_elaAwesome;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key && !string.IsNullOrEmpty(key))
            {
                var found = Application.Current.TryFindResource(key);
                if (found is FontFamily ff)
                    return ff;

                // 兜底：直接从文件创建 ElaAwesome
                if (key == "ElaAwesome" && s_elaAwesome == null)
                {
                    try
                    {
                        s_elaAwesome = new FontFamily(
                            new Uri("pack://application:,,,/Core.UI;component/Font/ElaAwesome.ttf"),
                            "ElaAwesome");
                    }
                    catch { }
                }
                if (key == "ElaAwesome" && s_elaAwesome != null)
                    return s_elaAwesome;

                System.Diagnostics.Debug.WriteLine($"[FontConverter] 未找到资源 '{key}'");
            }
            if (Application.Current.TryFindResource("FontAwesome") is FontFamily defaultFont)
                return defaultFont;
            return new FontFamily("Segoe UI");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
