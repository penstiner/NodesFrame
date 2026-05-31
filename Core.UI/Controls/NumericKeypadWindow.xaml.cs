using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Core.UI.Controls
{
    /// <summary>
    /// NumericKeypadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NumericKeypadWindow : UserControl
    {
        private Point _dragStartPoint;
        private bool _isDragging;

        private double _minValue;
        private double _maxValue;
        private bool _hasRange;

        public string ResultValue { get; private set; }

        public event Action<string> ValueSubmitted;
        public event Action Cancelled;
        public event Action<double, double> DragDeltaRequested;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NativePoint lpPoint);

        private static Point GetCursorScreenPoint()
        {
            GetCursorPos(out NativePoint p);
            return new Point(p.X, p.Y);
        }

        public NumericKeypadWindow()
        {
            InitializeComponent();
        }

        private void DragBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = GetCursorScreenPoint();
            Mouse.Capture((IInputElement)sender);
            e.Handled = true;
        }

        private void DragBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            Point current = GetCursorScreenPoint();
            double offsetX = current.X - _dragStartPoint.X;
            double offsetY = current.Y - _dragStartPoint.Y;
            _dragStartPoint = current;

            if (offsetX == 0 && offsetY == 0)
            {
                return;
            }

            DragDeltaRequested?.Invoke(offsetX, offsetY);
        }

        private void DragBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            Mouse.Capture(null);
            e.Handled = true;
        }

        public NumericKeypadWindow(string initialValue, string min, string max)
        {
            InitializeComponent();
            txtDisplay.Text = string.IsNullOrEmpty(initialValue) ? "0" : initialValue;
            ResultValue = txtDisplay.Text;
            txtLimits.Text = $"范围: {min} ~ {max}";

            _hasRange = double.TryParse(min, out _minValue) && double.TryParse(max, out _maxValue);
            if (!_hasRange)
            {
                txtLimits.Text = "范围: 未设置";
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtDisplay.Focus();
            txtDisplay.CaretIndex = txtDisplay.Text.Length;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Cancelled?.Invoke();
                return;
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SubmitValue();
            }
        }

        private void BtnNum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string num = btn.Content.ToString();
                string text = txtDisplay.Text;
                int caretPos = txtDisplay.SelectionStart;
                int selLen = txtDisplay.SelectionLength;

                // Remove selection if exists
                if (selLen > 0)
                {
                    text = text.Remove(caretPos, selLen);
                }

                // Prevent multiple decimal points
                if (num == ".")
                {
                    if (text.Contains("."))
                        return;
                }

                // Handle initial "0" state: replace rather than prefix
                if (text == "0" && num != ".")
                {
                    text = num;
                    caretPos = 1;
                }
                else
                {
                    text = text.Insert(caretPos, num);
                    caretPos += num.Length;
                }

                txtDisplay.Text = text;
                txtDisplay.CaretIndex = caretPos;
                ClearValidationState();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            string text = txtDisplay.Text;
            int caretPos = txtDisplay.SelectionStart;
            int selLen = txtDisplay.SelectionLength;

            if (selLen > 0)
            {
                // Delete selection
                text = text.Remove(caretPos, selLen);
            }
            else if (caretPos > 0)
            {
                // Delete character before caret
                text = text.Remove(caretPos - 1, 1);
                caretPos--;
            }

            if (string.IsNullOrEmpty(text) || text == "-")
            {
                text = "0";
                caretPos = 0;
            }

            txtDisplay.Text = text;
            txtDisplay.CaretIndex = caretPos;
            ClearValidationState();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDisplay.Text = "0";
            txtDisplay.CaretIndex = 0;
            ClearValidationState();
        }

        private void BtnSign_Click(object sender, RoutedEventArgs e)
        {
            string text = txtDisplay.Text;
            int caretPos = txtDisplay.CaretIndex;

            if (text.StartsWith("-"))
            {
                text = text.Substring(1);
                caretPos = Math.Max(0, caretPos - 1);
            }
            else
            {
                text = "-" + text;
                caretPos++;
            }

            txtDisplay.Text = text;
            txtDisplay.CaretIndex = caretPos;
            ClearValidationState();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }

        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            SubmitValue();
        }

        private void TxtDisplay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow digits, minus, and decimal point
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '-' && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            // For minus: only at position 0, only if none exists yet
            if (e.Text.Contains("-"))
            {
                if (txtDisplay.Text.Contains("-") || txtDisplay.SelectionStart != 0)
                {
                    e.Handled = true;
                    return;
                }
            }

            // For decimal point: only one allowed in the resulting text
            if (e.Text.Contains("."))
            {
                string textWithoutSelection = txtDisplay.Text.Remove(txtDisplay.SelectionStart, txtDisplay.SelectionLength);
                if (textWithoutSelection.Contains("."))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void SubmitValue()
        {
            if (!double.TryParse(txtDisplay.Text, out var input))
            {
                ShowValidationError("输入格式无效");
                return;
            }

            if (_hasRange && (input < _minValue || input > _maxValue))
            {
                ShowValidationError($"输入值必须在 {_minValue} ~ {_maxValue} 之间");
                return;
            }

            ResultValue = txtDisplay.Text;
            ClearValidationState();
            ValueSubmitted?.Invoke(ResultValue);
        }

        private void ShowValidationError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
            displayBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xD1, 0x34, 0x38));
        }

        private void ClearValidationState()
        {
            txtError.Visibility = Visibility.Collapsed;
            displayBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xE5));
        }
    }
}
