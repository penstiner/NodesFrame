using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shell.Models;
using Shell.Models.Attributes;

namespace Shell.Services
{
    /// <summary>
    /// 节点注册表：启动时通过反射扫描所有 [Node] 标记的 NodeViewModel 子类，
    /// 自动生成工具箱项、注册工厂函数、收集元数据。
    /// </summary>
    public static class NodeRegistry
    {
        private static readonly List<NodeMetaInfo> _nodes = new();
        private static bool _isInitialized;

        /// <summary>所有已注册节点的元数据。</summary>
        public static IReadOnlyList<NodeMetaInfo> RegisteredNodes
        {
            get
            {
                EnsureInitialized();
                return _nodes;
            }
        }

        /// <summary>
        /// 节点的元数据信息，包含工具箱显示所需的所有字段。
        /// </summary>
        public class NodeMetaInfo
        {
            /// <summary>节点的 CLR 类型。</summary>
            public Type NodeType { get; init; }

            /// <summary>序列化类型标识符。</summary>
            public string NodeTypeId { get; init; }

            /// <summary>工具箱分类。</summary>
            public string Category { get; init; }

            /// <summary>工具箱显示名称。</summary>
            public string DisplayName { get; init; }

            /// <summary>默认节点标题。</summary>
            public string DefaultTitle { get; init; }

            /// <summary>节点描述。</summary>
            public string Description { get; init; }

            /// <summary>节点头部覆盖色（十六进制字符串）。</summary>
            public string HeaderColor { get; init; }

            /// <summary>连接器元数据列表。</summary>
            public List<ConnectorMetaInfo> Connectors { get; init; } = new();

            /// <summary>创建节点实例的工厂函数。</summary>
            public Func<NodeViewModel> Factory { get; init; }
        }

        /// <summary>连接器元数据。</summary>
        public class ConnectorMetaInfo
        {
            public string Title { get; init; }
            public ConnectorDirection Direction { get; init; }
            public string ExpectedType { get; init; } = "Double";
            public string Description { get; init; } = string.Empty;
        }

        /// <summary>
        /// 初始化注册表（首次调用时自动触发，也可手动调用）。
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var nodeTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(NodeViewModel).IsAssignableFrom(t));

                foreach (var type in nodeTypes)
                {
                    var attr = type.GetCustomAttribute<NodeAttribute>();
                    if (attr == null) continue;

                    var typeId = attr.NodeTypeId ?? type.Name;
                    var category = attr.Category ?? "杂项";

                    // 收集连接器元数据
                    var connectorAttrs = type.GetCustomAttributes<NodeConnectorAttribute>();
                    var connectors = connectorAttrs.Select(ca => new ConnectorMetaInfo
                    {
                        Title = ca.Title,
                        Direction = ca.Direction,
                        ExpectedType = ca.ExpectedType ?? "Double",
                        Description = ca.Description
                    }).ToList();

                    var meta = new NodeMetaInfo
                    {
                        NodeType = type,
                        NodeTypeId = typeId,
                        Category = category,
                        DisplayName = attr.DisplayName ?? type.Name,
                        DefaultTitle = attr.DefaultTitle ?? type.Name,
                        Description = attr.Description ?? string.Empty,
                        HeaderColor = attr.HeaderColor,
                        Connectors = connectors,
                        Factory = () => (NodeViewModel)Activator.CreateInstance(type)
                    };

                    _nodes.Add(meta);

                    // 同步注册到 NodeFactory
                    NodeFactory.RegisterNodeType(typeId, () =>
                    {
                        var node = (NodeViewModel)Activator.CreateInstance(type);
                        node.Title = attr.DefaultTitle ?? type.Name;
                        return node;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NodeRegistry] 初始化失败: {ex.Message}");
            }

            _isInitialized = true;
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized) Initialize();
        }

        /// <summary>
        /// 根据 NodeTypeId 查找元数据。
        /// </summary>
        public static NodeMetaInfo FindByTypeId(string nodeTypeId)
        {
            EnsureInitialized();
            return _nodes.FirstOrDefault(n =>
                string.Equals(n.NodeTypeId, nodeTypeId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根据 CLR 类型查找元数据。
        /// </summary>
        public static NodeMetaInfo FindByType(Type nodeType)
        {
            EnsureInitialized();
            return _nodes.FirstOrDefault(n => n.NodeType == nodeType);
        }

        /// <summary>
        /// 按分类获取节点列表（用于工具箱生成）。
        /// </summary>
        public static IReadOnlyList<NodeMetaInfo> GetNodesByCategory(string category)
        {
            EnsureInitialized();
            return _nodes.Where(n =>
                string.Equals(n.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// 获取所有分类名（按注册顺序去重）。
        /// </summary>
        public static IReadOnlyList<string> GetAllCategories()
        {
            EnsureInitialized();
            return _nodes.Select(n => n.Category).Distinct().ToList();
        }
    }
}
