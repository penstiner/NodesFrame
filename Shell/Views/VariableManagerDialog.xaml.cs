using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Shell.Models;
using Shell.Services;

namespace Shell.Views
{
    public partial class VariableManagerDialog : Window
    {
        private readonly VariableManager _manager;

        public VariableManagerDialog(VariableManager manager)
        {
            InitializeComponent();
            _manager = manager;
            VariableList.ItemsSource = manager.Variables;
            UpdateCount();
        }

        private void UpdateCount()
            => VariableCount.Text = $"{_manager.Variables.Count} 个变量";

        // ── 类型切换时刷新值编辑器 ──
        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                var host = FindSiblingByName(cb, "ValueHost");
                if (host != null)
                    AttachEditor(host, host.DataContext as Variable);
            }
        }

        // ── 值编辑器加载 ──
        private void ValueHost_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ContentControl host && host.DataContext is Variable v)
            {
                AttachEditor(host, v);
                // 类型变更时刷新编辑器
                v.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(Variable.TypeName))
                        AttachEditor(host, v);
                };
            }
        }

        private static void AttachEditor(ContentControl host, Variable? v)
        {
            if (v == null) return;
            var handler = VariableTypeRegistry.Get(v.TypeName);
            host.Content = handler?.CreateEditor(v);
        }

        // ── 新增 ──
        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            int i = 1;
            while (_manager.Variables.Any(v => v.Name == $"变量{i}")) i++;
            _manager.AddVariable($"变量{i}", "Boolean", false);
            UpdateCount();
        }

        // ── 行内删除 ──
        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Variable v)
            {
                _manager.RemoveVariable(v);
                UpdateCount();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        public static void Show(VariableManager manager, Window owner)
            => new VariableManagerDialog(manager) { Owner = owner }.ShowDialog();

        // ── 辅助：在当前 Grid 行中查找指定名称的兄弟控件 ──
        private static ContentControl? FindSiblingByName(DependencyObject child, string name)
        {
            // 向上找到父 Grid（当前行容器）
            DependencyObject? parent = child;
            while (parent != null && parent is not Grid)
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);

            // 在 Grid 的 Children 中按名称查找
            if (parent is Grid grid)
            {
                foreach (var c in grid.Children)
                {
                    if (c is ContentControl cc && cc.Name == name)
                        return cc;
                }
            }
            return null;
        }
    }
}
