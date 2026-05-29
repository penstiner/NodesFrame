using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Core.UI.Converter
{
    /// <summary>
    /// Slider控件蓝色和灰色导轨占比转换器
    /// </summary>
    public class SliderValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 ||
                !(values[0] is double currentValue) ||
                !(values[1] is double actualWidth) ||
                !(values[2] is double min) ||
                !(values[3] is double max))
                return 0.0;

            if (actualWidth <= 0 || Math.Abs(max - min) < double.Epsilon)
                return 0.0;

            double ratio = (currentValue - min) / (max - min);
            return actualWidth * ratio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
