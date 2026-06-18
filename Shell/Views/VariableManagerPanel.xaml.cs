using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shell.Models;
using Shell.Services;

namespace Shell.Views
{
    public partial class VariableManagerPanel : UserControl
    {
        public VariableManagerPanel()
        {
            InitializeComponent();
        }

        private void ManageVariables_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindowViewModel;
            var doc = vm?.ActiveDocument;
            if (doc != null)
                VariableManagerDialog.Show(doc.VariableManager, Window.GetWindow(this));
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Variable v)
            {
                var vm = DataContext as ViewModels.MainWindowViewModel;
                if (vm != null)
                {
                    vm.SelectedVariable = v;
                    if (e.ClickCount == 2 && vm.ActiveDocument != null)
                        VariableManagerDialog.Show(vm.ActiveDocument.VariableManager, Window.GetWindow(this));
                }
            }
        }

        /// <summary>处理器驱动的值显示，监听 TypeName 变更自动刷新。</summary>
        private void ValueDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ContentControl host && host.DataContext is Variable v)
            {
                AttachDisplay(host, v);

                // 类型变更时刷新
                v.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(Variable.TypeName))
                        AttachDisplay(host, v);
                };
            }
        }

        private static void AttachDisplay(ContentControl host, Variable v)
        {
            var handler = VariableTypeRegistry.Get(v.TypeName);
            host.Content = handler?.CreateDisplay(v)
                ?? new TextBlock
                {
                    Text = v.ValueString,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 13
                };
        }
    }
}
