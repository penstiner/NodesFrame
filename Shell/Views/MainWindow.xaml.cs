using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shell.Models;
using Shell.Services;
using Shell.ViewModels;

namespace Shell.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 拖放数据格式标识
        private const string ToolBoxDragFormat = "Shell.ToolBoxItem";

        private Point? _dragStartPoint;

        /// <summary>日志面板是否已折叠。</summary>
        private bool _logCollapsed;

        /// <summary>节点编辑弹窗服务。</summary>
        private readonly INodeDialogService _dialogService = new NodeDialogService();

        public MainWindow()
        {
            InitializeComponent();

            // 绑定日志数据源
            LogItemsControl.ItemsSource = ExecutionLogger.Logs;

            // 日志自动滚动
            ExecutionLogger.LogAdded += entry =>
            {
                Dispatcher.Invoke(() => LogScrollViewer?.ScrollToEnd());
            };

            // 启动日志
            ExecutionLogger.Info("系统", "流程编辑器已启动");
        }

        // ═══════════════════════════════════════════
        //  工具箱拖拽事件
        // ═══════════════════════════════════════════

        private void ToolBoxTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item && item.DataContext is ToolBoxItem)
            {
                _dragStartPoint = e.GetPosition(null);
                item.IsSelected = true;
            }
            else
            {
                _dragStartPoint = null;
            }
        }

        private void ToolBoxTree_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = null;
        }

        private void ToolBoxTree_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragStartPoint == null)
                return;

            var currentPos = e.GetPosition(null);
            var diff = currentPos - _dragStartPoint.Value;

            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            if (sender is TreeViewItem item && item.DataContext is ToolBoxItem toolboxItem)
            {
                _dragStartPoint = null;
                DragDrop.DoDragDrop(item, toolboxItem, DragDropEffects.Copy);
            }
        }

        // ═══════════════════════════════════════════
        //  编辑器拖放事件
        // ═══════════════════════════════════════════

        private void Editor_DragOver(object sender, DragEventArgs e)
        {
            // 只处理来自工具箱的拖放，不拦截其他拖放事件（如 Nodify 内部操作）
            if (e.Data.GetDataPresent(typeof(ToolBoxItem)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void Editor_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;

            // 获取被拖放的工具箱项
            ToolBoxItem? item = null;
            if (e.Data.GetDataPresent(typeof(ToolBoxItem)))
                item = e.Data.GetData(typeof(ToolBoxItem)) as ToolBoxItem;
            else if (e.Data.GetDataPresent(ToolBoxDragFormat))
                item = e.Data.GetData(ToolBoxDragFormat) as ToolBoxItem;

            if (item == null) return;

            // 将拖放的屏幕坐标转换为编辑器图坐标
            Point screenPos = e.GetPosition(Editor);
            Point graphPos = new Point(
                screenPos.X / Editor.ViewportZoom + Editor.ViewportLocation.X,
                screenPos.Y / Editor.ViewportZoom + Editor.ViewportLocation.Y
            );

            vm.AddNodeFromToolBox(item, graphPos);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Ctrl+Z 撤销
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (DataContext is MainWindowViewModel vm && vm.UndoCommand.CanExecute(null))
                {
                    vm.UndoCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }

            // Ctrl+Y 重做
            if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (DataContext is MainWindowViewModel vm && vm.RedoCommand.CanExecute(null))
                {
                    vm.RedoCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show("是否要退出程序？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }

            // Del 键删除选中项
            if (e.Key == Key.Delete)
            {
                if (DataContext is MainWindowViewModel vm && vm.DeleteSelectedCommand.CanExecute(null))
                {
                    vm.DeleteSelectedCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }
        }

        // ═══════════════════════════════════════════
        //  连接点击选中
        // ═══════════════════════════════════════════

        private void Connection_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ConnectionViewModel conn)
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.ToggleConnectionSelection(conn);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 双击节点容器 → 弹出编辑窗口。
        /// </summary>
        private void ItemContainer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is NodeViewModel node)
            {
                _dialogService.ShowDialog(node);
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            ExecutionLogger.Clear();
        }

        // ═══════════════════════════════════════════
        //  日志面板折叠 / 展开
        // ═══════════════════════════════════════════

        private void LogCollapse_Click(object sender, RoutedEventArgs e)
        {
            _logCollapsed = !_logCollapsed;

            if (_logCollapsed)
            {
                EditorGrid.RowDefinitions[2].Height = new GridLength(30);
                LogCollapseBtn.Content = "\uf078";
                MenuCollapseIcon.Text = "\uf077";
                MenuCollapseText.Text = "  展开日志";
            }
            else
            {
                EditorGrid.RowDefinitions[2].Height = new GridLength(160);
                LogCollapseBtn.Content = "\uf077";
                MenuCollapseIcon.Text = "\uf078";
                MenuCollapseText.Text = "  折叠日志";
            }
        }

        // ═══════════════════════════════════════════
        //  图像预览缩放
        // ═══════════════════════════════════════════

        private void ImageZoomIn_Click(object sender, RoutedEventArgs e)
        {
            PreviewViewer.ZoomIn();
        }

        private void ImageZoomOut_Click(object sender, RoutedEventArgs e)
        {
            PreviewViewer.ZoomOut();
        }

        private void ImageFit_Click(object sender, RoutedEventArgs e)
        {
            PreviewViewer.ResetView();
        }

        private void ButtonMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}