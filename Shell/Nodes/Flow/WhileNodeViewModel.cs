using System.Threading;
using System.Threading.Tasks;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Flow
{
    [Node(
        Category = "流程控制",
        DisplayName = "循环判断",
        DefaultTitle = "循环判断",
        Description = "绑定布尔变量，为真时执行循环体，为假时等待或退出",
        NodeTypeId = "Flow.While")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "接收上游触发信号")]
    [NodeConnector(Title = "循环体", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "条件为真时进入循环体")]
    [NodeConnector(Title = "退出", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "停止或条件为假时走退出路径")]
    public class WhileNodeViewModel : NodeViewModel, ILoopNode
    {
        public WhileNodeViewModel()
        {
            Title = "循环判断";

            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });

            // Output[0] = 循环体
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "循环体",
                ExpectedType = System.TypeCode.Boolean
            });

            // Output[1] = 退出
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "退出",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        private string _conditionVariable = "";
        [NodeProperty(Key = "conditionVariable", DisplayName = "条件变量", Group = "循环设置")]
        public string ConditionVariable
        {
            get => _conditionVariable;
            set => SetProperty(ref _conditionVariable, value);
        }

        // ── 循环模式 ──
        private string _loopMode = "等待触发";
        [NodeProperty(Key = "loopMode", DisplayName = "循环模式", Group = "循环设置",
            Options = "等待触发,立即循环")]
        public string LoopMode
        {
            get => _loopMode;
            set => SetProperty(ref _loopMode, value);
        }

        /// <summary>
        /// 「等待触发」模式下的轮询间隔（毫秒）。
        /// 值越小响应越快，但 CPU 占用越高。默认 50ms。
        /// </summary>
        private int _pollIntervalMs = 50;
        [NodeProperty(Key = "pollIntervalMs", DisplayName = "轮询间隔(ms)", Group = "循环设置")]
        public int PollIntervalMs
        {
            get => _pollIntervalMs;
            set => SetProperty(ref _pollIntervalMs, Math.Max(10, value));
        }

        // IBranchNode / ILoopNode 实现
        public int ActiveOutputIndex { get; private set; }
        public bool IsLooping => ActiveOutputIndex == 0;
        public void OnLoopEnter() { CurrentIteration++; }
        public void OnLoopExit() { ResetIteration(); }
        public string LoopDescription => $"{Title} 第 {CurrentIteration} 轮循环";

        /// <summary>当前循环迭代计数（由 FlowExecutor 管理）。</summary>
        public int CurrentIteration { get; set; }

        /// <summary>重置迭代计数器（退出循环或流程开始时调用）。</summary>
        public void ResetIteration()
        {
            CurrentIteration = 0;
        }

        /// <summary>
        /// 同步执行：检查条件变量，设置 ActiveOutputIndex 和输出值。
        /// </summary>
        public override void Execute()
        {
            bool condition = EvaluateCondition();

            // 0 = 循环体（条件为真），1 = 退出（条件为假）
            ActiveOutputIndex = condition ? 0 : 1;

            Output[0].Value = condition ? VariantValue.FromBoolean(true) : VariantValue.Null;
            Output[1].Value = condition ? VariantValue.Null : VariantValue.FromBoolean(true);

            if (condition)
                ExecutionLogger.Info("循环判断", $"条件为 true，进入循环体");
            else
                ExecutionLogger.Info("循环判断", $"变量 '{ConditionVariable}' = false，退出循环");
        }

        /// <summary>
        /// 异步执行：在「等待触发」模式下，阻塞轮询直到条件变量为 true 或被取消。
        /// 「立即循环」模式下直接调用同步 Execute()。
        /// </summary>
        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (LoopMode == "等待触发" && !string.IsNullOrEmpty(ConditionVariable))
            {
                // ── 等待触发模式：轮询直到变量为 true 或被取消 ──
                ExecutionLogger.Info("循环判断",
                    $"⏳ 等待触发信号（变量: {ConditionVariable}，轮询间隔: {PollIntervalMs}ms）...");

                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    if (EvaluateCondition())
                    {
                        ExecutionLogger.Info("循环判断", "⚡ 收到触发信号");
                        break;
                    }

                    await Task.Delay(PollIntervalMs, ct);
                }
            }

            // 条件满足（或被取消前已满足），执行同步逻辑
            Execute();
        }

        /// <summary>
        /// 从 GlobalVariableManager 读取条件变量并返回布尔值。
        /// 变量不存在或类型不匹配时视为 true（兼容未配置变量的场景）。
        /// </summary>
        private bool EvaluateCondition()
        {
            if (string.IsNullOrEmpty(ConditionVariable))
                return true;

            var variable = GlobalVariableManager?.GetVariable(ConditionVariable);
            if (variable != null && variable.Value.TryGetBoolean(out var b))
                return b;

            // 变量不存在或无法解析为布尔值时，默认继续循环
            return true;
        }
    }
}
