using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Core.UI.Converter
{
    public class SafeLEDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush background = Brushes.Black;

            if ((bool)value)
            {
                background = Brushes.LightGreen;
            }
            else if (!(bool)value)
            {
                background = Brushes.Red;
            }
            return background;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
