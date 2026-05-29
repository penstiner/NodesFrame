using System;
using System.Collections.Generic;
using Shell.Models;
using Shell.Views;

namespace Shell.Services
{
    /// <summary>
    /// 节点编辑弹窗服务实现。
    /// 打开 NodeEditorWindow 并返回用户操作结果。
    /// </summary>
    public class NodeDialogService : INodeDialogService
    {
        /// <summary>
        /// 弹窗编辑节点。用户确认后在窗口内部通过绑定直接修改原节点属性。
        /// </summary>
        public bool ShowDialog(NodeViewModel node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var window = new NodeEditorWindow(node)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            var result = window.ShowDialog();
            return result == true;
        }
    }
}
