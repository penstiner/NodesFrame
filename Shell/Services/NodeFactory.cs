using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using Shell.Models;
using Shell.Models.Attributes;

namespace Shell.Services
{
    /// <summary>
    /// 节点工厂：根据 DTO 数据还原 NodeViewModel 实例。
    /// 支持硬编码类型映射 + 反射自动发现 [Node] 标记的节点。
    /// </summary>
    public class NodeFactory
    {
        // ── 工厂注册表：NodeTypeId → 创建函数 ──
        private static readonly Dictionary<string, Func<NodeViewModel>> _factoryRegistry = new(StringComparer.OrdinalIgnoreCase);
        private static bool _isInitialized;

        /// <summary>
        /// 根据 NodeData 创建对应的 NodeViewModel。
        /// </summary>
        public NodeViewModel CreateNode(NodeData data)
        {
            EnsureInitialized();

            NodeViewModel node = null;

            // 优先从注册表查找
            if (_factoryRegistry.TryGetValue(data.NodeType, out var factory))
            {
                node = factory();
            }

            if (node == null)
                return null;

            // 恢复通用属性（Id 必须恢复，否则连接数据无法匹配到节点）
            node.Id = data.Id;
            node.Title = data.Title;
            node.Location = new Point(data.X, data.Y);

            // 恢复节点特定属性
            RestoreProperties(node, data.Properties);

            return node;
        }

        /// <summary>
        /// 根据 NodeType 字符串创建空节点。
        /// </summary>
        public NodeViewModel CreateNode(string nodeType)
        {
            EnsureInitialized();

            if (_factoryRegistry.TryGetValue(nodeType, out var factory))
                return factory();

            return null;
        }

        /// <summary>
        /// 获取所有已注册的节点类型标识符。
        /// </summary>
        public static IReadOnlyCollection<string> GetRegisteredNodeTypes()
        {
            EnsureInitialized();
            return _factoryRegistry.Keys;
        }

        /// <summary>
        /// 手动注册一个节点类型。
        /// </summary>
        public static void RegisterNodeType(string nodeTypeId, Func<NodeViewModel> factory)
        {
            _factoryRegistry[nodeTypeId] = factory;
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // ── 注册内置节点类型 ──
            _factoryRegistry["Constant"] = () => new ConstantNodeViewModel { Title = "常量" };
            _factoryRegistry["Function"] = () => new FunctionNodeViewModel { Title = "加法" };
            _factoryRegistry["Display"] = () => new DisplayNodeViewModel { Title = "显示" };
            _factoryRegistry["Delay"] = () => new DelayNodeViewModel { Title = "延时" };
            _factoryRegistry["Condition"] = () => new ConditionNodeViewModel { Title = "判断" };
            _factoryRegistry["Loop"] = () => new LoopNodeViewModel { Title = "循环" };

            // ── 反射扫描 [Node] 属性标记的节点类型 ──
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
                    _factoryRegistry[typeId] = () =>
                    {
                        var instance = (NodeViewModel)Activator.CreateInstance(type);
                        instance.Title = attr.DefaultTitle ?? type.Name;
                        instance.NodeCategoryColor = !string.IsNullOrEmpty(attr.HeaderColor)
                            ? attr.HeaderColor
                            : (NodeViewModel.CategoryColors.TryGetValue(category, out var color) ? color : "#78909C");
                        return instance;
                    };
                }
            }
            catch
            {
                // 反射失败不影响内置节点
            }
        }

        private static void RestoreProperties(NodeViewModel node, Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0) return;

            // ── 反射恢复 [NodeProperty] 标记的属性 ──
            var type = node.GetType();
            var hasReflectionRestore = false;

            foreach (var prop in type.GetProperties())
            {
                var attr = prop.GetCustomAttribute<NodePropertyAttribute>();
                if (attr == null) continue;

                var key = attr.Key ?? prop.Name;
                if (!properties.TryGetValue(key, out var val)) continue;

                hasReflectionRestore = true;
                SetPropertyValue(node, prop, val);
            }

            if (hasReflectionRestore) return;

            // ── 向后兼容：硬编码节点类型属性恢复 ──
            if (node is ConstantNodeViewModel c)
            {
                if (properties.TryGetValue("constant", out var val))
                {
                    c.Constant = DeserializeVariantValue(val);
                }
            }
            else if (node is FunctionNodeViewModel f)
            {
                if (properties.TryGetValue("op", out var val) && val is JsonElement je2)
                {
                    var opStr = je2.GetString();
                    if (Enum.TryParse<FunctionOp>(opStr, out var op))
                        f.Op = op;
                }
            }
            else if (node is DelayNodeViewModel d)
            {
                if (properties.TryGetValue("delayMs", out var val) && val is JsonElement je3)
                    d.DelayMs = je3.GetInt32();
            }
            else if (node is ConditionNodeViewModel cond)
            {
                if (properties.TryGetValue("compareOp", out var val) && val is JsonElement je4)
                {
                    var opStr = je4.GetString();
                    if (Enum.TryParse<CompareOp>(opStr, out var op))
                        cond.Operation = op;
                }
                if (properties.TryGetValue("threshold", out var val2) && val2 is JsonElement je5)
                    cond.Threshold = je5.GetDouble();
            }
            else if (node is LoopNodeViewModel l)
            {
                if (properties.TryGetValue("loopCount", out var val) && val is JsonElement je6)
                    l.LoopCount = je6.GetInt32();
            }
        }

        private static void SetPropertyValue(NodeViewModel node, PropertyInfo prop, object rawVal)
        {
            try
            {
                if (prop.PropertyType == typeof(VariantValue))
                {
                    prop.SetValue(node, DeserializeVariantValue(rawVal));
                }
                else if (rawVal is JsonElement je)
                {
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var converted = JsonSerializer.Deserialize(je.GetRawText(), targetType);
                    prop.SetValue(node, converted);
                }
            }
            catch
            {
                // 属性恢复失败不影响节点创建
            }
        }

        /// <summary>
        /// 从序列化数据还原 VariantValue。
        /// </summary>
        internal static VariantValue DeserializeVariantValue(object rawVal)
        {
            if (rawVal == null) return VariantValue.Null;

            // 如果是 JsonElement，尝试解析复合格式 { typeCode, value }
            if (rawVal is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                string typeCode = null;
                JsonElement valueElement = default;

                if (je.TryGetProperty("typeCode", out var tc))
                    typeCode = tc.GetString();
                if (je.TryGetProperty("value", out var ve))
                    valueElement = ve;

                if (typeCode == null) return VariantValue.Null;

                return typeCode switch
                {
                    "Empty" => VariantValue.Null,
                    "Double" => valueElement.ValueKind != JsonValueKind.Null
                        ? VariantValue.FromDouble(valueElement.GetDouble())
                        : VariantValue.Null,
                    "Int32" => valueElement.ValueKind != JsonValueKind.Null
                        ? VariantValue.FromInt32(valueElement.GetInt32())
                        : VariantValue.Null,
                    "Boolean" => valueElement.ValueKind != JsonValueKind.Null
                        ? VariantValue.FromBoolean(valueElement.GetBoolean())
                        : VariantValue.Null,
                    "String" => valueElement.ValueKind != JsonValueKind.Null
                        ? VariantValue.FromString(valueElement.GetString())
                        : VariantValue.FromString(string.Empty),
                    "ByteArray" => valueElement.ValueKind != JsonValueKind.Null
                        ? VariantValue.FromBytes(Convert.FromBase64String(valueElement.GetString()))
                        : VariantValue.Null,
                    "DoubleArray" => valueElement.ValueKind == JsonValueKind.Array
                        ? VariantValue.FromDoubleArray(JsonSerializer.Deserialize<double[]>(valueElement.GetRawText()))
                        : VariantValue.Null,
                    _ => VariantValue.Null
                };
            }

            // 向后兼容：直接是数字 → double
            if (rawVal is JsonElement numJe)
            {
                if (numJe.ValueKind == JsonValueKind.Number)
                    return VariantValue.FromDouble(numJe.GetDouble());
                if (numJe.ValueKind == JsonValueKind.True || numJe.ValueKind == JsonValueKind.False)
                    return VariantValue.FromBoolean(numJe.GetBoolean());
                if (numJe.ValueKind == JsonValueKind.String)
                    return VariantValue.FromString(numJe.GetString());
            }

            return VariantValue.Null;
        }
    }
}
