using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Core.UI.Controls;
using Shell.Models;

namespace Shell.Services
{
    // ──── Boolean ────

    public class BooleanTypeHandler : IVariableTypeHandler
    {
        public string TypeName => "Boolean";

        public VariantValue Parse(string text, VariantValue fallback)
        {
            if (bool.TryParse(text, out var b))
                return VariantValue.FromBoolean(b);
            if (int.TryParse(text, out var i))
                return VariantValue.FromBoolean(i != 0);
            return fallback;
        }

        public string GetDisplayText(VariantValue value)
            => value.TryGetBoolean(out var b) && b ? "True" : "False";

        public FrameworkElement CreateEditor(Variable variable)
        {
            var cb = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                DataContext = variable
            };
            cb.SetBinding(CheckBox.IsCheckedProperty,
                new System.Windows.Data.Binding("ValueString")
                {
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
                    Converter = new Shell.Converter.StringToBoolConverter()
                });
            return cb;
        }

        public FrameworkElement CreateDisplay(Variable variable)
        {
            var cb = new CheckBox
            {
                IsHitTestVisible = false,
                Focusable = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                DataContext = variable
            };
            cb.SetBinding(CheckBox.IsCheckedProperty,
                new System.Windows.Data.Binding("ValueString")
                {
                    Mode = System.Windows.Data.BindingMode.OneWay,
                    Converter = new Shell.Converter.StringToBoolConverter()
                });
            return cb;
        }
    }

    // ──── Double ────

    public class DoubleTypeHandler : IVariableTypeHandler
    {
        public string TypeName => "Double";

        public VariantValue Parse(string text, VariantValue fallback)
            => double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                ? VariantValue.FromDouble(d) : fallback;

        public string GetDisplayText(VariantValue value)
            => value.TryGetDouble(out var d) ? d.ToString("G", CultureInfo.InvariantCulture) : "0";

        public FrameworkElement CreateEditor(Variable variable)
            => CreateNumEditor(variable, NumberKinds.Float);

        public FrameworkElement CreateDisplay(Variable variable)
        {
            var tb = new TextBlock
            {
                DataContext = variable,
                TextAlignment = TextAlignment.Center,
                FontSize = 13, FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.LimeGreen
            };
            tb.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("ValueString") { Mode = System.Windows.Data.BindingMode.OneWay });
            return tb;
        }

        private static FrameworkElement CreateNumEditor(Variable variable, NumberKinds kind)
        {
            var box = new NumericTextBox
            {
                DataContext = variable,
                NumberKind = kind,
                EnableKeypad = false,
                MinWidth = 50,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF3F3F58"),
                Foreground = System.Windows.Media.Brushes.LimeGreen,
                FontSize = 14, FontWeight = FontWeights.Bold,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CaretBrush = System.Windows.Media.Brushes.LimeGreen,
                Padding = new Thickness(4, 2, 4, 2)
            };
            box.SetBinding(TextBox.TextProperty,
                new System.Windows.Data.Binding("ValueString")
                {
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                });
            return box;
        }
    }

    // ──── Int32 ────

    public class Int32TypeHandler : IVariableTypeHandler
    {
        public string TypeName => "Int32";

        public VariantValue Parse(string text, VariantValue fallback)
            => int.TryParse(text, out var i) ? VariantValue.FromInt32(i) : fallback;

        public string GetDisplayText(VariantValue value)
            => value.TryGetInt32(out var i) ? i.ToString() : "0";

        public FrameworkElement CreateEditor(Variable variable)
        {
            var box = new NumericTextBox
            {
                DataContext = variable,
                NumberKind = NumberKinds.Int,
                EnableKeypad = false,
                MinWidth = 50,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF3F3F58"),
                Foreground = System.Windows.Media.Brushes.LimeGreen,
                FontSize = 14, FontWeight = FontWeights.Bold,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CaretBrush = System.Windows.Media.Brushes.LimeGreen,
                Padding = new Thickness(4, 2, 4, 2)
            };
            box.SetBinding(TextBox.TextProperty,
                new System.Windows.Data.Binding("ValueString")
                {
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                });
            return box;
        }

        public FrameworkElement CreateDisplay(Variable variable)
        {
            var tb = new TextBlock
            {
                DataContext = variable,
                TextAlignment = TextAlignment.Center,
                FontSize = 13, FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.LimeGreen
            };
            tb.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("ValueString") { Mode = System.Windows.Data.BindingMode.OneWay });
            return tb;
        }
    }

    // ──── String ────

    public class StringTypeHandler : IVariableTypeHandler
    {
        public string TypeName => "String";

        public VariantValue Parse(string text, VariantValue fallback)
            => VariantValue.FromString(text);

        public string GetDisplayText(VariantValue value)
            => value.TryGetString(out var s) ? s : "";

        public FrameworkElement CreateEditor(Variable variable)
        {
            var box = new TextBox
            {
                DataContext = variable,
                MinWidth = 60,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = System.Windows.Media.Brushes.Gray,
                Foreground = System.Windows.Media.Brushes.LightGray,
                FontSize = 13,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CaretBrush = System.Windows.Media.Brushes.LightGray,
                Padding = new Thickness(4, 2, 4, 2)
            };
            box.SetBinding(TextBox.TextProperty,
                new System.Windows.Data.Binding("ValueString")
                {
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                });
            return box;
        }

        public FrameworkElement CreateDisplay(Variable variable)
        {
            var tb = new TextBlock
            {
                DataContext = variable,
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.LightGray,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            tb.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("ValueString") { Mode = System.Windows.Data.BindingMode.OneWay });
            return tb;
        }
    }
}
