using System;

namespace Shell.Models.Attributes
{
    /// <summary>
    /// 标记一个 NodeViewModel 子类，使其能被 NodeRegistry 自动发现和注册。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeAttribute : Attribute
    {
        /// <summary>节点在工具箱中的分类。</summary>
        public string Category { get; set; } = "杂项";

        /// <summary>工具箱中显示的名称。</summary>
        public string DisplayName { get; set; }

        /// <summary>默认节点标题。</summary>
        public string DefaultTitle { get; set; }

        /// <summary>描述文本。</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>节点类型标识符（用于序列化）。若未指定则使用类名。</summary>
        public string NodeTypeId { get; set; }
    }

    /// <summary>
    /// 标记节点上需要被序列化/反序列化的属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class NodePropertyAttribute : Attribute
    {
        /// <summary>序列化键名，默认使用属性名。</summary>
        public string Key { get; set; }

        /// <summary>是否为必填属性。</summary>
        public bool Required { get; set; }

        /// <summary>编辑器中显示的友好名称（留空则使用属性名）。</summary>
        public string DisplayName { get; set; }

        /// <summary>属性分组名（相同 Group 归入同一可折叠区域）。</summary>
        public string Group { get; set; }

        /// <summary>下拉选项标签（逗号分隔）。设置后渲染为 ComboBox 而非数字框。</summary>
        public string Options { get; set; }
    }

    /// <summary>
    /// 标记节点的连接器声明（用于工具箱提示和文档生成）。
    /// 仅作为元数据标记，实际连接器由构造函数创建。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class NodeConnectorAttribute : Attribute
    {
        /// <summary>连接器显示标题。</summary>
        public string Title { get; set; }

        /// <summary>连接器方向。</summary>
        public ConnectorDirection Direction { get; set; }

        /// <summary>期望的数据类型（TypeCode 字符串形式）。</summary>
        public string ExpectedType { get; set; } = "Double";

        /// <summary>连接器描述。</summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>连接器方向。</summary>
    public enum ConnectorDirection
    {
        Input,
        Output
    }
}
