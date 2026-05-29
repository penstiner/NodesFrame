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

        /// <summary>FontAwesome 图标字符码，如 "\uf017" 表示时钟</summary>
        public string IconCode { get; set; } = "\uf111";

        /// <summary>图标字体族资源Key，如 "FontAwesome" 或 "iconfont"</summary>
        public string IconFontFamily { get; set; } = "FontAwesome";

        /// <summary>类型颜色标签，十六进制颜色值，用于左侧色条显示</summary>
        public string ColorTag { get; set; } = "#FF42A5F5";

        public override string ToString() => DisplayName;
    }
}
