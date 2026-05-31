namespace Shell.Models
{
    /// <summary>分支节点接口：Execute 后通过 ActiveOutputIndex 指示流式执行器应走哪个输出。</summary>
    public interface IBranchNode
    {
        /// <summary>执行后的活跃输出端口索引。</summary>
        int ActiveOutputIndex { get; }
    }
}
