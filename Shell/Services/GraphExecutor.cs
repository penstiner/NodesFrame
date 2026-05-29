using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shell.Models;

namespace Shell.Services
{
    /// <summary>
    /// 图执行器：基于拓扑排序（Kahn 算法）按依赖顺序执行节点计算。
    /// 支持环检测和错误反馈。
    /// </summary>
    public class GraphExecutor : IGraphExecutor
    {
        /// <summary>
        /// 执行结果。
        /// </summary>
        public class ExecutionResult
        {
            /// <summary>是否执行成功（无环且无异常）。</summary>
            public bool Success { get; set; } = true;

            /// <summary>拓扑排序后的执行顺序。</summary>
            public List<NodeViewModel> ExecutionOrder { get; set; } = new List<NodeViewModel>();

            /// <summary>参与循环依赖的节点（若存在环）。</summary>
            public List<NodeViewModel> CyclicNodes { get; set; } = new List<NodeViewModel>();

            /// <summary>错误信息（如有）。</summary>
            public string ErrorMessage { get; set; } = string.Empty;
        }

        /// <summary>
        /// 对给定的节点和连接执行拓扑排序并逐一执行。
        /// </summary>
        /// <param name="nodes">图中所有节点。</param>
        /// <param name="connections">图中所有连接。</param>
        /// <returns>执行结果。</returns>
        public ExecutionResult Execute(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections)
        {
            var result = new ExecutionResult();

            if (nodes == null || nodes.Count == 0)
                return result;

            try
            {
                // 1. 构建连接器 → 节点 快速查找字典
                var connectorToNode = new Dictionary<ConnectorViewModel, NodeViewModel>();
                foreach (var node in nodes)
                {
                    foreach (var input in node.Input)
                        connectorToNode[input] = node;
                    foreach (var output in node.Output)
                        connectorToNode[output] = node;
                }

                // 2. 构建邻接表 和 入度表
                //    若连接 A.Output → B.Input，则 A 必须先于 B 执行（A → B）
                var adjacency = new Dictionary<NodeViewModel, List<NodeViewModel>>();
                var inDegree = new Dictionary<NodeViewModel, int>();

                foreach (var node in nodes)
                {
                    adjacency[node] = new List<NodeViewModel>();
                    inDegree[node] = 0;
                }

                if (connections != null)
                {
                    foreach (var conn in connections)
                    {
                        if (connectorToNode.TryGetValue(conn.Source, out var sourceNode) &&
                            connectorToNode.TryGetValue(conn.Target, out var targetNode) &&
                            sourceNode != targetNode)
                        {
                            adjacency[sourceNode].Add(targetNode);
                            inDegree[targetNode]++;
                        }
                    }
                }

                // 3. Kahn 拓扑排序
                var queue = new Queue<NodeViewModel>();
                foreach (var node in nodes)
                {
                    if (inDegree[node] == 0)
                        queue.Enqueue(node);
                }

                var sorted = new List<NodeViewModel>();

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    sorted.Add(current);

                    foreach (var neighbor in adjacency[current])
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }

                // 4. 环检测：若未全部排序，存在环
                if (sorted.Count != nodes.Count)
                {
                    var processed = new HashSet<NodeViewModel>(sorted);
                    result.CyclicNodes = nodes.Where(n => !processed.Contains(n)).ToList();
                    result.Success = false;
                    result.ErrorMessage = $"图中存在循环依赖，涉及 {result.CyclicNodes.Count} 个节点。";
                    return result;
                }

                result.ExecutionOrder = sorted;

                // 5. 按拓扑序执行（含耗时统计 + 状态指示）
                foreach (var node in nodes)
                {
                    node.ExecutionTime = null;
                    node.State = ExecutionState.Idle;
                }

                var sw = Stopwatch.StartNew();
                foreach (var node in sorted)
                {
                    node.State = ExecutionState.Running;
                    var nodeSw = Stopwatch.StartNew();
                    try
                    {
                        node.Execute();
                    }
                    catch (Exception ex)
                    {
                        nodeSw.Stop();
                        node.ExecutionTime = nodeSw.Elapsed;
                        node.State = ExecutionState.Error;
                        ExecutionLogger.Error("执行器", $"✖ {node.Title} — {node.ExecutionTimeDisplay}: {ex.Message}");
                        continue;
                    }
                    nodeSw.Stop();
                    node.ExecutionTime = nodeSw.Elapsed;
                    node.State = ExecutionState.Success;
                    ExecutionLogger.Info("执行器", $"▶ {node.Title} — {node.ExecutionTimeDisplay}");
                }
                sw.Stop();
                ExecutionLogger.Success("执行器", $"执行完成，共 {sorted.Count} 个节点，总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"执行异常：{ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 异步执行：拓扑排序后按依赖顺序异步执行每个节点。
        /// 支持 CancellationToken 取消。
        /// </summary>
        public async Task<ExecutionResult> ExecuteAsync(
            IReadOnlyList<NodeViewModel> nodes,
            IReadOnlyList<ConnectionViewModel> connections,
            CancellationToken ct = default)
        {
            var result = new ExecutionResult();

            if (nodes == null || nodes.Count == 0)
                return result;

            try
            {
                // 1. 构建连接器 → 节点映射
                var connectorToNode = new Dictionary<ConnectorViewModel, NodeViewModel>();
                foreach (var node in nodes)
                {
                    foreach (var input in node.Input)
                        connectorToNode[input] = node;
                    foreach (var output in node.Output)
                        connectorToNode[output] = node;
                }

                // 2. 构建邻接表和入度表
                var adjacency = new Dictionary<NodeViewModel, List<NodeViewModel>>();
                var inDegree = new Dictionary<NodeViewModel, int>();

                foreach (var node in nodes)
                {
                    adjacency[node] = new List<NodeViewModel>();
                    inDegree[node] = 0;
                }

                if (connections != null)
                {
                    foreach (var conn in connections)
                    {
                        if (connectorToNode.TryGetValue(conn.Source, out var sourceNode) &&
                            connectorToNode.TryGetValue(conn.Target, out var targetNode) &&
                            sourceNode != targetNode)
                        {
                            adjacency[sourceNode].Add(targetNode);
                            inDegree[targetNode]++;
                        }
                    }
                }

                // 3. Kahn 拓扑排序
                var queue = new Queue<NodeViewModel>();
                foreach (var node in nodes)
                {
                    if (inDegree[node] == 0)
                        queue.Enqueue(node);
                }

                var sorted = new List<NodeViewModel>();

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    sorted.Add(current);

                    foreach (var neighbor in adjacency[current])
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }

                // 4. 环检测
                if (sorted.Count != nodes.Count)
                {
                    var processed = new HashSet<NodeViewModel>(sorted);
                    result.CyclicNodes = nodes.Where(n => !processed.Contains(n)).ToList();
                    result.Success = false;
                    result.ErrorMessage = $"图中存在循环依赖，涉及 {result.CyclicNodes.Count} 个节点。";
                    return result;
                }

                result.ExecutionOrder = sorted;

                // 5. 按拓扑序异步执行（含耗时统计 + 状态指示）
                foreach (var node in nodes)
                {
                    node.ExecutionTime = null;
                    node.State = ExecutionState.Idle;
                }

                var sw = Stopwatch.StartNew();
                foreach (var node in sorted)
                {
                    ct.ThrowIfCancellationRequested();
                    node.State = ExecutionState.Running;
                    var nodeSw = Stopwatch.StartNew();
                    try
                    {
                        await node.ExecuteAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        nodeSw.Stop();
                        node.ExecutionTime = nodeSw.Elapsed;
                        node.State = ExecutionState.Error;
                        ExecutionLogger.Error("执行器", $"✖ {node.Title} — {node.ExecutionTimeDisplay}: {ex.Message}");
                        throw;
                    }
                    nodeSw.Stop();
                    node.ExecutionTime = nodeSw.Elapsed;
                    node.State = ExecutionState.Success;
                    ExecutionLogger.Info("执行器", $"▶ {node.Title} — {node.ExecutionTimeDisplay}");
                }
                sw.Stop();
                ExecutionLogger.Success("执行器", $"执行完成，共 {sorted.Count} 个节点，总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = "执行已被取消。";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"执行异常：{ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 快速检查在现有图中添加一条连接是否会形成环。
        /// </summary>
        /// <param name="nodes">所有节点。</param>
        /// <param name="existingConnections">已有连接。</param>
        /// <param name="newSource">新连接的源连接器。</param>
        /// <param name="newTarget">新连接的目标连接器。</param>
        /// <returns>若形成环返回 true。</returns>
        public bool WouldCreateCycle(
            IReadOnlyList<NodeViewModel> nodes,
            IReadOnlyList<ConnectionViewModel> existingConnections,
            ConnectorViewModel newSource,
            ConnectorViewModel newTarget)
        {
            // 如果源和目标属于同一节点 → 形成环
            if (newSource.ParentNode == newTarget.ParentNode)
                return true;

            // 从目标节点 BFS/DFS 查找是否可以到达源节点 → 如果可以则形成环
            var visited = new HashSet<NodeViewModel>();
            var queue = new Queue<NodeViewModel>();
            var targetNode = newTarget.ParentNode;
            var sourceNode = newSource.ParentNode;

            if (targetNode == null || sourceNode == null)
                return true;

            // 构建连接器到节点的映射
            var connectorToNode = new Dictionary<ConnectorViewModel, NodeViewModel>();
            foreach (var node in nodes)
            {
                foreach (var output in node.Output)
                    connectorToNode[output] = node;
            }

            // 从 targetNode 出发，沿现有连接向下游走
            queue.Enqueue(targetNode);
            visited.Add(targetNode);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // 查找从 current 出发的所有连接
                var downstream = (existingConnections ?? Enumerable.Empty<ConnectionViewModel>())
                    .Where(c => c.Source.ParentNode == current);

                foreach (var conn in downstream)
                {
                    var nextNode = conn.Target.ParentNode;
                    if (nextNode == null) continue;

                    if (nextNode == sourceNode)
                        return true; // 找到回路

                    if (visited.Add(nextNode))
                        queue.Enqueue(nextNode);
                }
            }

            return false;
        }
    }
}
