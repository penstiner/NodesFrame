using System.Windows;
using System.Windows.Input;
using Shell.ViewModels;

namespace Shell.Views
{
    public partial class HardwareConfigWindow : Window
    {
        private readonly HardwareConfigViewModel _vm;

        public HardwareConfigWindow()
        {
            InitializeComponent();
            _vm = new HardwareConfigViewModel();
            DataContext = _vm;
        }

        // 拖拽标题栏移动窗口
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) DragMove();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SaveToCard();
            _vm.SaveToFile();
            DialogResult = true;
            Close();
        }
    }
}
