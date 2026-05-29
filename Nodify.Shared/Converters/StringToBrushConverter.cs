using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Nodify
{
    /// <summary>
    /// 将十六进制颜色字符串（如 "#FF42A5F5"）转换为 SolidColorBrush。
    /// </summary>
    public class StringToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorStr && !string.IsNullOrEmpty(colorStr))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorStr);
                    return new SolidColorBrush(color);
                }
                catch { }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
