using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Shell.Models;
using Shell.Models.Attributes;

namespace Shell.Services
{
    #region DTO

    public class GraphData
    {
        [JsonPropertyName("nodes")]
        public List<NodeData> Nodes { get; set; } = new List<NodeData>();

        [JsonPropertyName("connections")]
        public List<ConnectionData> Connections { get; set; } = new List<ConnectionData>();
    }

    public class NodeData
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("type")]
        public string NodeType { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        /// <summary>
        /// 节点特定属性（如 FunctionOp、常量值等）。
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    /// <summary>
    /// 图序列化器：将 Nodes + Connections 序列化为 JSON，或从 JSON 反序列化。
    /// 仅保存逻辑图结构，不保存运行时状态（Anchor, IsConnected 等）。
    /// </summary>
    public class GraphSerializer : IGraphSerializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        /// <summary>
        /// 将图的节点和连接序列化为 JSON 字符串。
        /// </summary>
        public string Serialize(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections)
        {
            var graph = new GraphData();

            foreach (var node in nodes)
            {
                var nd = new NodeData
                {
                    Id = node.Id,
                    Title = node.Title,
                    X = node.Location.X,
                    Y = node.Location.Y
                };

                nd.NodeType = DetermineNodeType(node);
                nd.Properties = ExtractProperties(node);

                graph.Nodes.Add(nd);
            }

            foreach (var conn in connections)
            {
                var sourceNode = conn.Source.ParentNode;
                var targetNode = conn.Target.ParentNode;
                if (sourceNode == null || targetNode == null) continue;

                var sourceIdx = sourceNode.Output.IndexOf(conn.Source);
                var targetIdx = targetNode.Input.IndexOf(conn.Target);
                if (sourceIdx < 0 || targetIdx < 0) continue;

                graph.Connections.Add(new ConnectionData
                {
                    SourceNodeId = sourceNode.Id,
                    SourceConnectorIndex = sourceIdx,
                    TargetNodeId = targetNode.Id,
                    TargetConnectorIndex = targetIdx
                });
            }

            return JsonSerializer.Serialize(graph, _jsonOptions);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化，返回 Tuple(节点列表, 连接数据列表)。
        /// 连接数据需要由调用方配合 NodeFactory 重建 ConnectionViewModel。
        /// </summary>
        public (List<NodeViewModel> nodes, List<ConnectionData> connections) Deserialize(string json)
        {
            var graph = JsonSerializer.Deserialize<GraphData>(json, _jsonOptions);
            if (graph == null)
                throw new InvalidOperationException("无法解析 JSON 数据。");

            var factory = new NodeFactory();
            var nodes = new List<NodeViewModel>();

            foreach (var nd in graph.Nodes)
            {
                var node = factory.CreateNode(nd);
                if (node != null)
                    nodes.Add(node);
            }

            return (nodes, graph.Connections);
        }

        /// <summary>
        /// 根据 ViewModel 类型返回节点类型字符串。
        /// 优先从 NodeRegistry 查找 [Node] 属性标记的 NodeTypeId，
        /// 再回退到硬编码类型判断（向后兼容）。
        /// </summary>
        private static string DetermineNodeType(NodeViewModel node)
        {
            // 从 NodeRegistry 查找元数据
            var meta = NodeRegistry.FindByType(node.GetType());
            if (meta != null)
                return meta.NodeTypeId;

            // 向后兼容：硬编码类型判断
            return node switch
            {
                ConstantNodeViewModel => "Constant",
                FunctionNodeViewModel => "Function",
                DisplayNodeViewModel => "Display",
                DelayNodeViewModel => "Delay",
                ConditionNodeViewModel => "Condition",
                LoopNodeViewModel => "Loop",
                _ => node.GetType().Name
            };
        }

        /// <summary>
        /// 提取节点特定属性用于序列化。
        /// 优先使用 [NodeProperty] 标记的属性反射提取，
        /// 其次回退到硬编码类型判断（向后兼容）。
        /// </summary>
        private static Dictionary<string, object> ExtractProperties(NodeViewModel node)
        {
            var props = new Dictionary<string, object>();

            // ── 反射提取 [NodeProperty] 标记的属性 ──
            var type = node.GetType();
            foreach (var prop in type.GetProperties())
            {
                var attr = prop.GetCustomAttributes(typeof(NodePropertyAttribute), true)
                    .FirstOrDefault() as NodePropertyAttribute;
                if (attr == null) continue;

                var key = attr.Key ?? prop.Name;
                var val = prop.GetValue(node);
                if (val is VariantValue vv)
                {
                    // 序列化 VariantValue 为复合对象
                    props[key] = SerializeVariantValue(vv);
                }
                else
                {
                    props[key] = val;
                }
            }

            // ── 向后兼容：硬编码节点类型属性提取 ──
            if (props.Count == 0)
            {
                if (node is ConstantNodeViewModel c)
                {
                    props["constant"] = SerializeVariantValue(c.Constant);
                }
                else if (node is FunctionNodeViewModel f)
                {
                    props["op"] = f.Op.ToString();
                }
                else if (node is DelayNodeViewModel d)
                {
                    props["delayMs"] = d.DelayMs;
                }
                else if (node is ConditionNodeViewModel cond)
                {
                    props["compareOp"] = cond.Operation.ToString();
                    props["threshold"] = cond.Threshold;
                }
                else if (node is LoopNodeViewModel l)
                {
                    props["loopCount"] = l.LoopCount;
                }
            }

            return props;
        }

        /// <summary>
        /// 将 VariantValue 序列化为字典格式 { typeCode, value }。
        /// </summary>
        private static object SerializeVariantValue(VariantValue vv)
        {
            if (vv.IsNull)
                return new Dictionary<string, object> { ["typeCode"] = "Empty", ["value"] = null };

            var raw = vv.GetRawValue();
            // byte[] 转为 Base64 字符串
            if (vv.TypeCode == TypeCode.Object && raw is byte[] bytes)
            {
                return new Dictionary<string, object>
                {
                    ["typeCode"] = "ByteArray",
                    ["value"] = Convert.ToBase64String(bytes)
                };
            }
            if (vv.TypeCode == TypeCode.Object && raw is double[] dArr)
            {
                return new Dictionary<string, object>
                {
                    ["typeCode"] = "DoubleArray",
                    ["value"] = dArr
                };
            }

            return new Dictionary<string, object>
            {
                ["typeCode"] = vv.TypeCode.ToString(),
                ["value"] = raw
            };
        }
    }
}
