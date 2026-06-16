using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shell.Models;
using Shell.Models.Nodes.Flow;
using Shell.Models.Nodes.Hardware;

namespace Shell.Services
{
    /// <summary>
    /// 流式执行引擎：从 FlowStart 出发，沿连接边逐节点执行。
    /// 支持循环栈（WhileNode/LoopNode）、旁路分支、条件分支、
    /// CancellationToken 取消及清理路径。
    /// </summary>
    public class FlowExecutor
    {
        public VariableManager? VariableManager { get; set; }
        public int LoopDelayMs { get; set; } = 0;
        public int TotalLoopIterations { get; private set; }

        // ═══════════════════════════════════════════════
        //  RunAsync（主入口）
        // ═══════════════════════════════════════════════

        public async Task<FlowExecutionResult> RunAsync(
            IReadOnlyList<NodeViewModel> nodes,
            IReadOnlyList<ConnectionViewModel> connections,
            CancellationToken ct = default)
        {
            var result = new FlowExecutionResult();
            if (nodes == null || nodes.Count == 0) return result.Fail("节点列表为空。");

            var startNode = nodes.OfType<FlowStartNodeViewModel>().FirstOrDefault();
            if (startNode == null) return result.Fail("未找到流程开始节点。");

            ResetAllNodes(nodes);
            var outputMap = BuildOutputMap(connections);

            var sw = Stopwatch.StartNew();
            var loopStack = new Stack<NodeViewModel>();
            var sideBranchTasks = new List<Task>();
            TotalLoopIterations = 0;
            NodeViewModel? current = startNode;

            try
            {
                while (current != null)
                {
                    if (ct.IsCancellationRequested) { result.WasCancelled = true; ExecutionLogger.Warning("流程执行器", "⏹ 检测到取消请求，退出主循环"); break; }

                    // ① 执行
                    if (!await TryExecuteNode(current, ct, result)) break;
                    // ② 传播 + 旁路 + 输出绑定
                    PropagateOutputs(current, outputMap);
                    WriteOutputBindings(current);
                    RunSidePaths(current, outputMap, result, ct, sideBranchTasks);
                    // ③ 循环栈
                    ManageLoopStack(current, loopStack);
                    // ④ 下一节点
                    var next = ResolveNextNode(current, outputMap);
                    if (next is FlowEndNodeViewModel endNode)
                    {
                        ExecuteInline(endNode);
                        result.ExecutedNodeCount++;
                        ExecutionLogger.Info("流程执行器", $"▶ {endNode.Title} — {endNode.ExecutionTimeDisplay}");
                        break;
                    }
                    // ⑤ 回跳
                    next = await TryLoopBack(next, loopStack, ct);
                    current = next;
                    if (current != null) await Task.Yield();
                }
            }
            catch (Exception ex) { result.Fail($"流程执行异常: {ex.Message}"); }
            sw.Stop();

            // 等待所有旁路分支完成（或被取消）
            if (sideBranchTasks.Count > 0)
            {
                try { await Task.WhenAll(sideBranchTasks); }
                catch { /* 单个分支异常已在 ExecuteSideBranchesAsync 内部处理 */ }
            }

            if (result.WasCancelled)
            {
                ExecutionLogger.Warning("流程执行器", "流程已取消，正在执行清理路径...");
                await ExecuteCleanupPathAsync(nodes, outputMap);
                result.Fail("执行已被取消。");
            }

            // 取消后：将所有 Running / Error 节点重置为 Idle，耗时清零
            // 非取消的失败：只重置 Running（保留 Error 让用户看到哪个节点出错）
            if (result.WasCancelled)
            {
                foreach (var n in nodes)
                {
                    if (n.State == ExecutionState.Running || n.State == ExecutionState.Error)
                    {
                        n.State = ExecutionState.Idle;
                        n.ExecutionTime = null;
                    }
                }
            }
            else if (!result.Success)
            {
                foreach (var n in nodes)
                {
                    if (n.State == ExecutionState.Running)
                    {
                        n.State = ExecutionState.Idle;
                        n.ExecutionTime = null;
                    }
                }
            }

            if (result.Success)
                ExecutionLogger.Success("流程执行器",
                    $"流程执行完成，共 {result.ExecutedNodeCount} 个节点，总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms");
            else if (!result.WasCancelled)
                ExecutionLogger.Error("流程执行器", $"流程执行失败: {result.ErrorMessage}");

            return result;
        }

        // ═══════════════════════════════════════════════
        //  ① TryExecuteNode
        // ═══════════════════════════════════════════════

        private async Task<bool> TryExecuteNode(NodeViewModel node, CancellationToken ct,
            FlowExecutionResult result)
        {
            var vm = VariableManager ?? NodeViewModel.GlobalVariableManager;
            node.ResolveBindings(vm);
            node.State = ExecutionState.Running;

            var sw = Stopwatch.StartNew();
            try { await node.ExecuteAsync(ct); }
            catch (OperationCanceledException)
            { node.ExecutionTime = sw.Elapsed; node.State = ExecutionState.Idle; result.WasCancelled = true; ExecutionLogger.Warning("流程执行器", $"⏹ 节点 [{node.Title}] 收到取消，正在退出..."); return false; }
            catch (Exception ex)
            {
                node.ExecutionTime = sw.Elapsed; node.State = ExecutionState.Error;
                result.Fail($"节点 [{node.Title}] 执行异常: {ex.Message}");
                ExecutionLogger.Error("流程执行器", $"✖ {node.Title} — {node.ExecutionTimeDisplay}: {ex.Message}");
                return false;
            }
            // 如果节点内部已将状态标记为 Error，则视为执行失败
            if (node.State == ExecutionState.Error)
            {
                node.ExecutionTime = sw.Elapsed;
                result.Fail($"节点 [{node.Title}] 执行失败（节点内部标记）。");
                ExecutionLogger.Error("流程执行器", $"✖ {node.Title} — {node.ExecutionTimeDisplay}: 节点内部错误");
                return false;
            }
            node.ExecutionTime = sw.Elapsed; node.State = ExecutionState.Success;
            result.ExecutedNodeCount++;
            ExecutionLogger.Info("流程执行器", $"▶ {node.Title} — {node.ExecutionTimeDisplay}");
            return true;
        }

        private void ExecuteInline(NodeViewModel node)
        {
            node.State = ExecutionState.Running; var sw = Stopwatch.StartNew();
            node.Execute(); node.ExecutionTime = sw.Elapsed; node.State = ExecutionState.Success;
            WriteOutputBindings(node);
        }

        // ═══════════════════════════════════════════════
        //  ② PropagateOutputs / RunSidePaths / WriteOutputBindings
        // ═══════════════════════════════════════════════

        private static void PropagateOutputs(NodeViewModel node,
            Dictionary<ConnectorViewModel, List<ConnectionViewModel>> outputMap)
        {
            foreach (var output in node.Output)
                if (outputMap.TryGetValue(output, out var conns))
                    foreach (var conn in conns) conn.Target.Value = output.Value;
        }

        /// <summary>
        /// 将节点的输出值写入已绑定的全局变量。
        /// 在 PropagateOutputs 之后调用，确保变量拿到最新计算结果。
        /// </summary>
        private void WriteOutputBindings(NodeViewModel node)
        {
            var vm = VariableManager ?? NodeViewModel.GlobalVariableManager;
            if (vm == null || node.OutputBindingDict.Count == 0) return;

            for (int i = 0; i < node.Output.Count; i++)
            {
                if (!node.OutputBindingDict.TryGetValue(i.ToString(), out var binding))
                    continue;
                if (!binding.IsBound || string.IsNullOrEmpty(binding.BoundVariableName))
                    continue;

                var variable = vm.GetVariable(binding.BoundVariableName);
                if (variable != null)
                {
                    var outputValue = node.Output[i].Value;
                    variable.SetValueAndNotify(outputValue);
                }
            }
        }

        private void RunSidePaths(NodeViewModel current,
            Dictionary<ConnectorViewModel, List<ConnectionViewModel>> outputMap,
            FlowExecutionResult result, CancellationToken ct,
            List<Task> sideBranchTasks)
        {
            int outIdx = current is IBranchNode b ? b.ActiveOutputIndex : 0;

            // 活跃输出 → 旁路：跳过第一条连接（它是主路径）
            if (outIdx >= 0 && outIdx < current.Output.Count
                && outputMap.TryGetValue(current.Output[outIdx], out var activeConns)
                && activeConns.Count > 1)
            {
                bool isFirst = true;
                foreach (var conn in activeConns)
                {
                    if (isFirst) { isFirst = false; continue; }
                    var side = conn.Target.ParentNode;
                    if (IsLoopNode(side) || side == current) continue;
                    // 启动并行任务执行旁路分支，加入追踪列表
                    sideBranchTasks.Add(Task.Run(() => ExecuteSideBranchesAsync(side!, outputMap, result, ct)));
                }
            }
            // 非活跃输出 → 全部旁路（但跳过 IBranchNode 的 Output[1]——那是退出/停止口）
            for (int i = 0; i < current.Output.Count; i++)
            {
                if (i == outIdx) continue;
                // IBranchNode 的 Output[1] 是退出/停止路径，正常流程不执行
                if (current is IBranchNode && i == 1) continue;
                if (!outputMap.TryGetValue(current.Output[i], out var pConns)) continue;
                foreach (var conn in pConns)
                {
                    var side = conn.Target.ParentNode;
                    if (side == null || side == current) continue;
                    sideBranchTasks.Add(Task.Run(() => ExecuteSideBranchesAsync(side, outputMap, result, ct)));
                }
            }
        }

        private async Task ExecuteSideBranchesAsync(NodeViewModel start,
            Dictionary<ConnectorViewModel, List<ConnectionViewModel>> outputMap,
            FlowExecutionResult result, CancellationToken ct)
        {
            var queue = new Queue<NodeViewModel>();
            var visited = new HashSet<NodeViewModel>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (!visited.Add(node)) continue;
                if (IsLoopNode(node) || node is FlowEndNodeViewModel) continue;
                try
                {
                    node.State = ExecutionState.Running;
                    var sw = Stopwatch.StartNew();
                    // 支持异步执行并响应取消
                    await node.ExecuteAsync(ct);
                    node.ExecutionTime = sw.Elapsed;
                    node.State = ExecutionState.Success;
                    result.ExecutedNodeCount++;
                    PropagateOutputs(node, outputMap);
                    WriteOutputBindings(node);
                    ExecutionLogger.Info("流程执行器", $"├ {node.Title} — {node.ExecutionTimeDisplay}");
                }
                catch (OperationCanceledException)
                {
                    node.ExecutionTime = TimeSpan.Zero;
                    node.State = ExecutionState.Idle;
                    ExecutionLogger.Info("流程执行器", $"├ {node.Title} 已取消");
                    // 取消后：将该分支已访问过的 Error 节点也重置
                    foreach (var vn in visited)
                    {
                        if (vn.State == ExecutionState.Error)
                        {
                            vn.State = ExecutionState.Idle;
                            vn.ExecutionTime = null;
                        }
                    }
                    return;
                }
                catch (Exception ex)
                {
                    node.State = ExecutionState.Error;
                    node.ExecutionTime = TimeSpan.Zero;
                    ExecutionLogger.Warning("流程执行器", $"├ {node.Title} 旁路异常: {ex.Message}");
                    continue;
                }
                foreach (var output in node.Output)
                {
                    if (!outputMap.TryGetValue(output, out var conns)) continue;
                    foreach (var conn in conns)
                    {
                        var t = conn.Target.ParentNode;
                        if (t != null && !visited.Contains(t)) queue.Enqueue(t);
                    }
                }
            }
        }

        private static bool IsLoopNode(NodeViewModel? n) =>
            n is ILoopNode;

        // ═══════════════════════════════════════════════
        //  ③ ManageLoopStack
        // ═══════════════════════════════════════════════

        private void ManageLoopStack(NodeViewModel current, Stack<NodeViewModel> loopStack)
        {
            if (current is not ILoopNode loop) return;

            if (loop.IsLooping)
            {
                loopStack.Push(current);
                loop.OnLoopEnter();
                TotalLoopIterations++;
                ExecutionLogger.Info("流程执行器", $"↻ {loop.LoopDescription}");
            }
            else
            {
                RemoveFromLoopStack(loopStack, current);
                loop.OnLoopExit();
                ExecutionLogger.Info("流程执行器", $"✓ {current.Title} 退出循环");
            }
        }

        private static void RemoveFromLoopStack(Stack<NodeViewModel> stack, NodeViewModel target)
        {
            if (stack.Count == 0) return;
            if (stack.Peek() == target) { stack.Pop(); return; }
            var temp = new List<NodeViewModel>();
            while (stack.Count > 0) { var item = stack.Pop(); if (item == target) break; temp.Add(item); }
            for (int i = temp.Count - 1; i >= 0; i--) stack.Push(temp[i]);
        }

        // ═══════════════════════════════════════════════
        //  ④ ResolveNextNode
        // ═══════════════════════════════════════════════

        private static NodeViewModel? ResolveNextNode(NodeViewModel current,
            Dictionary<ConnectorViewModel, List<ConnectionViewModel>> outputMap)
        {
            if (current.Output.Count == 0) return null;
            int idx = current is IBranchNode b ? b.ActiveOutputIndex : 0;
            if (idx < 0 || idx >= current.Output.Count) return null;
            if (!outputMap.TryGetValue(current.Output[idx], out var conns) || conns.Count == 0) return null;
            var priority = conns.FirstOrDefault(c => IsLoopNode(c.Target.ParentNode));
            return (priority ?? conns[0]).Target.ParentNode;
        }

        // ═══════════════════════════════════════════════
        //  ⑤ TryLoopBack
        // ═══════════════════════════════════════════════

        private async Task<NodeViewModel?> TryLoopBack(NodeViewModel? next,
            Stack<NodeViewModel> loopStack, CancellationToken ct)
        {
            if (next != null || loopStack.Count == 0) return next;
            if (ct.IsCancellationRequested) return null;
            if (LoopDelayMs > 0) await Task.Delay(LoopDelayMs, ct);
            var back = loopStack.Peek();
            ExecutionLogger.Info("流程执行器", $"↻ 循环回到 [{back.Title}]（已执行 {TotalLoopIterations} 轮）");
            return back;
        }

        // ═══════════════════════════════════════════════
        //  初始化
        // ═══════════════════════════════════════════════

        private static void ResetAllNodes(IReadOnlyList<NodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                node.State = ExecutionState.Idle; node.ExecutionTime = null;
                if (node is ILoopNode loop) loop.OnLoopExit();
            }
        }

        private static Dictionary<ConnectorViewModel, List<ConnectionViewModel>> BuildOutputMap(
            IReadOnlyList<ConnectionViewModel>? connections)
        {
            var map = new Dictionary<ConnectorViewModel, List<ConnectionViewModel>>();
            if (connections == null) return map;
            foreach (var conn in connections)
            { if (!map.TryGetValue(conn.Source, out var list)) map[conn.Source] = list = new List<ConnectionViewModel>(); list.Add(conn); }
            return map;
        }

        /// <summary>检测添加连接后是否会形成循环依赖（简单 DFS）。</summary>
        public static bool WouldCreateCycle(IReadOnlyList<NodeViewModel> nodes,
            IReadOnlyList<ConnectionViewModel> connections,
            ConnectorViewModel source, ConnectorViewModel target)
        {
            var adj = new Dictionary<NodeViewModel, List<NodeViewModel>>();
            foreach (var n in nodes) adj[n] = new List<NodeViewModel>();
            foreach (var c in connections)
            {
                var s = c.Source.ParentNode;
                var t = c.Target.ParentNode;
                if (s != null && t != null) adj[s].Add(t);
            }
            // 模拟添加新连接
            var srcNode = source.ParentNode;
            var tgtNode = target.ParentNode;
            if (srcNode != null && tgtNode != null)
                adj[srcNode].Add(tgtNode);

            // DFS 检测从 target 出发是否能回到自身
            var visited = new HashSet<NodeViewModel>();
            var stack = new Stack<NodeViewModel>();
            stack.Push(tgtNode!);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (cur == tgtNode && visited.Contains(cur)) return true;
                if (!visited.Add(cur)) continue;
                foreach (var nb in adj[cur]) stack.Push(nb);
            }
            return false;
        }

        // ═══════════════════════════════════════════════
        //  清理
        // ═══════════════════════════════════════════════

        private async Task ExecuteCleanupPathAsync(
            IReadOnlyList<NodeViewModel> nodes,
            Dictionary<ConnectorViewModel, List<ConnectionViewModel>> outputMap)
        {
            try
            {
                // 策略1：找 IBranchNode，优先 Output[1]（停止口），其次 Output[0]
                var branchNodes = nodes
                    .Where(n => n is IBranchNode && n.State != ExecutionState.Error).ToList();
                NodeViewModel? cleanupStart = null; int cleanupPort = 0;
                foreach (var bn in branchNodes)
                {
                    if (bn.Output.Count > 1 && outputMap.TryGetValue(bn.Output[1], out var c1) && c1.Count > 0)
                    { cleanupStart = bn; cleanupPort = 1; break; }
                    if (bn.Output.Count > 0 && outputMap.TryGetValue(bn.Output[0], out var c0) && c0.Count > 0)
                    { cleanupStart = bn; cleanupPort = 0; break; }
                }

                if (cleanupStart != null)
                {
                    var exit = cleanupStart.Output[cleanupPort];
                    if (outputMap.TryGetValue(exit, out var exitConns) && exitConns.Count > 0)
                    {
                        var node = exitConns[0].Target.ParentNode;
                        while (node != null && node is not FlowEndNodeViewModel)
                        {
                            if (node.State == ExecutionState.Idle)
                            {
                                node.State = ExecutionState.Running;
                                try
                                {
                                    var sw = Stopwatch.StartNew();
                                    await node.ExecuteAsync(CancellationToken.None);
                                    node.ExecutionTime = sw.Elapsed;
                                    node.State = ExecutionState.Success;
                                    ExecutionLogger.Info("流程执行器", $"▶ [清理] {node.Title} — {node.ExecutionTimeDisplay}");
                                }
                                catch (Exception ex)
                                {
                                    node.ExecutionTime = TimeSpan.Zero;
                                    node.State = ExecutionState.Error;
                                    ExecutionLogger.Error("流程执行器", $"✖ [清理] {node.Title}: {ex.Message}");
                                }
                                PropagateOutputs(node, outputMap);
                                WriteOutputBindings(node);
                            }
                            node = ResolveNextNode(node, outputMap);
                        }
                        return;
                    }
                }

                // 策略2：兜底执行所有 CameraCloseNode
                foreach (var cn in nodes.OfType<CameraCloseNodeViewModel>()
                    .Where(n => n.State == ExecutionState.Idle))
                {
                    cn.State = ExecutionState.Running;
                    try
                    {
                        await cn.ExecuteAsync(CancellationToken.None);
                        cn.State = ExecutionState.Success;
                        ExecutionLogger.Info("流程执行器", $"▶ [清理] {cn.Title}");
                    }
                    catch (Exception ex)
                    {
                        cn.State = ExecutionState.Error;
                        ExecutionLogger.Error("流程执行器", $"✖ [清理] {cn.Title}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ExecutionLogger.Error("流程执行器", $"清理路径执行异常: {ex.Message}");
            }
        }
    }

    /// <summary>流式执行结果。</summary>
    public class FlowExecutionResult
    {
        public bool Success { get; set; } = true;
        public bool WasCancelled { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int ExecutedNodeCount { get; set; }

        public FlowExecutionResult Fail(string msg) { Success = false; ErrorMessage = msg; return this; }
    }
}
