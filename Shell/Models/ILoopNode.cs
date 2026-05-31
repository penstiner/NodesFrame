namespace Shell.Models
{
    /// <summary>
    /// 循环节点接口：统一 WhileNode / LoopNode / WaitSignalNode 的循环语义。
    /// 实现此接口的节点可被 FlowExecutor 统一管理循环栈。
    /// </summary>
    public interface ILoopNode : IBranchNode
    {
        /// <summary>当前是否处于循环状态（ActiveOutputIndex == 0 且满足循环条件）。</summary>
        bool IsLooping { get; }

        /// <summary>进入循环时回调（计数器自增、日志等）。</summary>
        void OnLoopEnter();

        /// <summary>退出循环时回调（计数器重置等）。</summary>
        void OnLoopExit();

        /// <summary>循环日志描述文本（如 "WhileNode 第 3 轮循环"）。</summary>
        string LoopDescription { get; }
    }
}
