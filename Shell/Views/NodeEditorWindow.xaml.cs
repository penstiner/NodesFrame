using System;
using System.Windows;
using Shell.Models;

namespace Shell.Views
{
    /// <summary>
    /// 节点编辑弹窗。编辑模板在 Style/NodeEditTemplates.xaml 中定义。
    /// </summary>
    public partial class NodeEditorWindow : Window
    {
        public NodeEditorWindow(NodeViewModel node)
        {
            InitializeComponent();
            DataContext = node ?? throw new ArgumentNullException(nameof(node));
            Title = $"节点设定 — {node.Title}";
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
                DragMove();
        }
    }
}
