using Shell.Models;

namespace Shell.Services
{
    /// <summary>
    /// 节点编辑弹窗服务接口。
    /// </summary>
    public interface INodeDialogService
    {
        /// <summary>
        /// 弹出节点编辑窗口。
        /// </summary>
        /// <param name="node">待编辑的节点 ViewModel（将编辑其副本，确认后提交）。</param>
        /// <returns>用户确认修改返回 true，取消返回 false。</returns>
        bool ShowDialog(NodeViewModel node);
    }
}
