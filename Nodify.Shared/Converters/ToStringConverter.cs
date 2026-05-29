using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Nodify
{
    public class ToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point p)
            {
                return $"{p.X:0.0}, {p.Y:0.0}";
            }

            if (value is Size s)
            {
                return $"{s.Width:0.0}, {s.Height:0.0}";
            }

            if (value is double d)
            {
                return d.ToString("0.00");
            }

            if (value is Key key)
            {
                return FormatKey(key);
            }

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        private static string FormatKey(Key key)
        {
            switch (key)
            {
                case Key.CapsLock: return "Caps Lock";
                case Key.PageUp: return "Page Up";
                case Key.PageDown: return "Page Down";
                case Key.PrintScreen: return "Print Screen";
                case Key.LWin: return "Left Win";
                case Key.RWin: return "Right Win";
                case Key.Apps: return "Menu";
                case Key.D0: return "0";
                case Key.D1: return "1";
                case Key.D2: return "2";
                case Key.D3: return "3";
                case Key.D4: return "4";
                case Key.D5: return "5";
                case Key.D6: return "6";
                case Key.D7: return "7";
                case Key.D8: return "8";
                case Key.D9: return "9";
                case Key.NumPad0: return "Num 0";
                case Key.NumPad1: return "Num 1";
                case Key.NumPad2: return "Num 2";
                case Key.NumPad3: return "Num 3";
                case Key.NumPad4: return "Num 4";
                case Key.NumPad5: return "Num 5";
                case Key.NumPad6: return "Num 6";
                case Key.NumPad7: return "Num 7";
                case Key.NumPad8: return "Num 8";
                case Key.NumPad9: return "Num 9";
                case Key.Multiply: return "Num *";
                case Key.Add: return "Num +";
                case Key.Separator: return "Num Separator";
                case Key.Subtract: return "Num -";
                case Key.Decimal: return "Num .";
                case Key.Divide: return "Num /";
                case Key.NumLock: return "Num Lock";
                case Key.Scroll: return "Scroll Lock";
                case Key.LeftShift: return "Left Shift";
                case Key.RightShift: return "Right Shift";
                case Key.LeftCtrl: return "Left Ctrl";
                case Key.RightCtrl: return "Right Ctrl";
                case Key.LeftAlt: return "Left Alt";
                case Key.RightAlt: return "Right Alt";
                case Key.OemSemicolon: return ";";
                case Key.OemPlus: return "=";
                case Key.OemComma: return ",";
                case Key.OemMinus: return "-";
                case Key.OemPeriod: return ".";
                case Key.OemQuestion: return "/";
                case Key.OemTilde: return "`";
                case Key.OemOpenBrackets: return "[";
                case Key.OemPipe: return "\\";
                case Key.OemCloseBrackets: return "]";
                case Key.OemQuotes: return "'";
                case Key.OemBackslash: return "\\";
                case Key.Play: return "Play";
                case Key.Zoom: return "Zoom";
                default: return key.ToString();
            }
        }
    }
}
