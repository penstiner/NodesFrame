using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Core.UI.Controls
{
    public enum NumberKinds
    {
        Default,
        Int,
        Float
    }

    public class NumericTextBox : TextBox
    {
        private static System.Windows.Controls.Primitives.Popup _sharedPopup;
        private static NumericTextBox _keypadTarget;
        private static bool _isKeypadOpen;

        public NumericTextBox()
        {
            //PMaxValue = "10000";
            //PMinValue = "0";
        }

        public static readonly DependencyProperty EnableKeypadProperty =
            DependencyProperty.Register("EnableKeypad", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));

        public bool EnableKeypad
        {
            get { return (bool)GetValue(EnableKeypadProperty); }
            set { SetValue(EnableKeypadProperty, value); }
        }

        public NumberKinds NumberKind
        {
            get { return (NumberKinds)GetValue(NumberKindProperty); }
            set { SetValue(NumberKindProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberKind.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberKindProperty =
            DependencyProperty.Register("NumberKind", typeof(NumberKinds), typeof(NumericTextBox), new PropertyMetadata(NumberKinds.Int));




        /// <summary>
        /// 最大值
        /// </summary>
        public string PMaxValue
        {
            get { return (string)GetValue(PMaxValueProperty); }
            set { SetValue(PMaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PMaxValueProperty =
            DependencyProperty.Register("PMaxValue", typeof(string), typeof(NumericTextBox), new PropertyMetadata("100000000"));

        /// <summary>
        /// 最小值
        /// </summary>
        public string PMinValue
        {
            get { return (string)GetValue(PMinValueProperty); }
            set { SetValue(PMinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PMinValueProperty =
            DependencyProperty.Register("PMinValue", typeof(string), typeof(NumericTextBox), new PropertyMetadata("-100000000"));

        private string val = "0";
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_isKeypadOpen && this == _keypadTarget)
            {
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
            string txt = this.Text;
            int index = this.CaretIndex;

            if (Text.Contains("."))
            {
                if (txt.Split('.')[1].Length >= 5 && index > txt.Split('.')[0].Length && this.SelectionLength == 0)//控制小数点后输入位数
                {
                    e.Handled = true;
                    return;
                }
            }
            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                if (NumberKind == NumberKinds.Float)
                {
                    val = ".";
                }
                else
                {

                    val = "";
                }
            }

            //屏蔽非法按键
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal || e.Key == Key.Subtract || e.Key.ToString() == "Tab")
            {
                if (txt.Contains(".") && e.Key == Key.Decimal)
                {
                    e.Handled = true;
                    return;
                }
                else if ((txt.Contains("-") || this.CaretIndex != 0) && e.Key == Key.Subtract)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else if (((e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod || e.Key == Key.OemMinus) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                if (txt.Contains(".") && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                    return;
                }
                else if ((txt.Contains("-") || this.CaretIndex != 0) && e.Key == Key.OemMinus)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                //this.Text = "";
                if (e.Key.ToString() != "RightCtrl")
                { }
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (_isKeypadOpen && this == _keypadTarget)
            {
                e.Handled = true;
                return;
            }

            // 构造输入后的预期文本（考虑选中替换）
            string current = this.Text;
            string future;
            if (SelectionLength > 0)
                future = current.Remove(SelectionStart, SelectionLength).Insert(CaretIndex, e.Text);
            else
                future = current.Insert(CaretIndex, e.Text);

            // 验证每个字符是否为合法数字字符
            string[] numberStr = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-", "." };
            bool allValid = true;
            for (int i = 0; i < future.Length; i++)
            {
                if (!numberStr.Contains(future[i].ToString()))
                { allValid = false; break; }
            }

            if (!allValid)
            {
                e.Handled = true;
                return;
            }

            // 仅允许一个 "-" 且必须在开头
            if (future.IndexOf('-', 1) >= 0)
            { e.Handled = true; return; }
            // 仅允许一个小数点
            if (future.IndexOf('.') != future.LastIndexOf('.'))
            { e.Handled = true; return; }
            // Int 模式禁止小数点
            if (NumberKind == NumberKinds.Int && future.Contains('.'))
            { e.Handled = true; return; }
            // "." 开头补 0
            if (future.StartsWith(".", StringComparison.Ordinal))
            { e.Handled = true; this.Text = "0" + future; return; }
            // "-." 开头补 0
            if (future.StartsWith("-.", StringComparison.Ordinal))
            { e.Handled = true; this.Text = "-0" + future.Substring(1); return; }

            // 合法输入，放行（绑定会在 Text 变化后自动触发）
            e.Handled = false;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (EnableKeypad)
            {
                e.Handled = true;
                ShowSharedKeypad();
                return;
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        private void ShowSharedKeypad()
        {
            if (_sharedPopup == null)
            {
                _sharedPopup = new System.Windows.Controls.Primitives.Popup
                {
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                    StaysOpen = true,
                    AllowsTransparency = true
                };
            }

            var target = this;
            var keypad = new NumericKeypadWindow(target.Text, target.PMinValue, target.PMaxValue);

            _sharedPopup.Child = keypad;
            _sharedPopup.PlacementTarget = target;
            _sharedPopup.HorizontalOffset = 0;
            _sharedPopup.VerticalOffset = 0;
            _sharedPopup.IsOpen = true;

            _isKeypadOpen = true;
            _keypadTarget = target;
            target.Dispatcher.BeginInvoke(new Action(() =>
            {
                target.Focus();
                target.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);

            keypad.ValueSubmitted += async (val) =>
            {
                CloseSharedKeypad();
                await target.OnVirtualKeypadValueSubmittedAsync(val);
            };

            keypad.Cancelled += () =>
            {
                CloseSharedKeypad();
            };

            keypad.DragDeltaRequested += (offsetX, offsetY) =>
            {
                if (_sharedPopup == null || !_sharedPopup.IsOpen)
                {
                    return;
                }

                _sharedPopup.HorizontalOffset = Math.Round(_sharedPopup.HorizontalOffset + offsetX, 0);
                _sharedPopup.VerticalOffset = Math.Round(_sharedPopup.VerticalOffset + offsetY, 0);
            };
        }

        private static void CloseSharedKeypad()
        {
            _isKeypadOpen = false;
            _keypadTarget = null;

            if (_sharedPopup == null)
            {
                return;
            }

            _sharedPopup.IsOpen = false;
            _sharedPopup.Child = null;
        }

        protected virtual Task OnVirtualKeypadValueSubmittedAsync(string value)
        {
            this.Text = value;
            var binding = System.Windows.Data.BindingOperations.GetBindingExpression(this, TextProperty);
            binding?.UpdateSource();
            return Task.CompletedTask;
        }

        private bool _suppressTextChanged;

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;
            base.OnTextChanged(e);

            if (double.TryParse(this.Text, out double value))
            {
                double.TryParse(PMaxValue, out double max);
                if (value > max)
                {
                    _suppressTextChanged = true;
                    this.Text = max.ToString();
                    _suppressTextChanged = false;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (double.TryParse(this.Text, out double value))
            {
                double.TryParse(PMinValue, out double min);
                double.TryParse(PMaxValue, out double max);
                if (value < min)
                {
                    _suppressTextChanged = true;
                    this.Text = min.ToString();
                    _suppressTextChanged = false;
                }
                if (value > max)
                {
                    _suppressTextChanged = true;
                    this.Text = max.ToString();
                    _suppressTextChanged = false;
                }
            }
        }
    }
}
