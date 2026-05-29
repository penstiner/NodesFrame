namespace Shell.Models
{
    /// <summary>
    /// 工具箱中的节点类型描述符。
    /// </summary>
    public class ToolBoxItem
    {
        /// <summary>节点类型标识，用于 NodeFactory 创建对应节点。</summary>
        public string NodeType { get; set; } = string.Empty;

        /// <summary>工具箱中显示的名称。</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>默认节点标题。</summary>
        public string DefaultTitle { get; set; } = string.Empty;

        /// <summary>描述文本。</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>分类标签（可选）。</summary>
        public string Category { get; set; } = string.Empty;

        public override string ToString() => DisplayName;
    }
}
