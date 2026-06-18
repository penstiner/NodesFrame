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

        /// <summary>节点头部覆盖色（十六进制字符串，如 "#26C6DA"）。留空则使用分类默认色。</summary>
        public string HeaderColor { get; set; }

        /// <summary>节点类型标识符（用于序列化）。若未指定则使用类名。</summary>
        public string NodeTypeId { get; set; }

        /// <summary>旧版类型标识符列表。反序列化旧存档时自动映射到当前类型。</summary>
        public string[] LegacyTypeIds { get; set; } = Array.Empty<string>();
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

        /// <summary>
        /// 动态选项来源方法名。指定节点上一个返回 IReadOnlyList&lt;string&gt; 的方法名，
        /// PropertyItem 将在构造时调用该方法获取下拉选项（优先级高于 Options）。
        /// </summary>
        public string DynamicOptionsSource { get; set; }

        /// <summary>是否允许绑定到变量。</summary>
        public bool BindableToVariable { get; set; } = true;

        /// <summary>
        /// 若为 true，ResolveBindings 不会用变量值覆盖此属性。
        /// 适用于属性存储的是变量名（而非变量值）的场景。
        /// </summary>
        public bool SkipBindingResolve { get; set; } = false;

        /// <summary>属性描述文本。</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>数值最小值（仅 Number 类型有效）。</summary>
        public double Min { get; set; } = double.MinValue;

        /// <summary>数值最大值（仅 Number 类型有效）。</summary>
        public double Max { get; set; } = double.MaxValue;
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
