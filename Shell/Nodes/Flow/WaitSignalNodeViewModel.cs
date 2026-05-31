using System;
using System.Threading;
using System.Threading.Tasks;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Flow
{
    [Node(
        Category = "流程控制",
        DisplayName = "等待信号 ⏳",
        DefaultTitle = "等待信号[单次]",
        Description = "绑定布尔变量，阻塞等待触发信号后放行；支持循环/单次模式",
        NodeTypeId = "Flow.WaitSignal")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "上游触发入口")]
    [NodeConnector(Title = "收到信号", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "收到触发信号后放行")]
    [NodeConnector(Title = "超时/停止", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "流程停止时走此路径")]
    public class WaitSignalNodeViewModel : NodeViewModel, ILoopNode
    {
        public WaitSignalNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = TypeCode.Boolean
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "收到信号",
                ExpectedType = TypeCode.Boolean
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "超时/停止",
                ExpectedType = TypeCode.Boolean
            });
            UpdateTitle();
        }

        // IBranchNode / ILoopNode 实现
        public int ActiveOutputIndex { get; private set; }
        public bool IsLooping => ActiveOutputIndex == 0 && ExecutionMode == "循环执行";
        public void OnLoopEnter() { /* SignalCount 在 ExecuteAsync 中已自增 */ }
        public void OnLoopExit() { ResetSignalCount(); }
        public string LoopDescription => $"{Title} 第 {SignalCount} 次触发";

        private string _signalVariable = "";
        [NodeProperty(Key = "signalVariable", DisplayName = "信号变量", Group = "信号设置",
            SkipBindingResolve = true, Description = "选择要监听的 Boolean 变量")]
        public string SignalVariable
        {
            get => _signalVariable;
            set => SetProperty(ref _signalVariable, value);
        }

        private string _executionMode = "单次执行";
        [NodeProperty(Key = "executionMode", DisplayName = "执行模式", Group = "信号设置",
            Options = "循环执行,单次执行")]
        public string ExecutionMode
        {
            get => _executionMode;
            set { if (SetProperty(ref _executionMode, value)) UpdateTitle(); }
        }

        // ── 轮询间隔 ──
        private int _pollIntervalMs = 50;
        [NodeProperty(Key = "pollIntervalMs", DisplayName = "轮询间隔(ms)", Group = "信号设置")]
        public int PollIntervalMs
        {
            get => _pollIntervalMs;
            set => SetProperty(ref _pollIntervalMs, Math.Max(10, value));
        }

        /// <summary>累计收到信号的次数。</summary>
        public int SignalCount { get; set; }

        /// <summary>重置信号计数。</summary>
        public void ResetSignalCount() => SignalCount = 0;

        /// <summary>
        /// 同步执行：直接检查信号变量，设置输出。
        /// （通常由 ExecuteAsync 在等待完成后调用）
        /// </summary>
        public override void Execute()
        {
            bool hasSignal = EvaluateSignal();

            // 0 = 收到信号（走拍照流程），1 = 超时/停止（走清理路径）
            ActiveOutputIndex = hasSignal ? 0 : 1;

            Output[0].Value = hasSignal ? VariantValue.FromBoolean(true) : VariantValue.Null;
            Output[1].Value = hasSignal ? VariantValue.Null : VariantValue.FromBoolean(true);
        }

        /// <summary>
        /// 异步执行：阻塞轮询信号变量直到为 true 或被取消。
        /// 被取消时设置 ActiveOutputIndex=1（停止路径）并抛出异常，
        /// 由 FlowExecutor 捕获后走清理路径。
        /// </summary>
        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var signalName = ResolvedSignalName;
            if (string.IsNullOrEmpty(signalName))
            {
                Execute();
                return;
            }

            try
            {
                // 轮询等待信号（每次迭代检查取消令牌，不占 CPU）
                bool hasSignal = EvaluateSignal();
                while (!EvaluateSignal())
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(PollIntervalMs, ct);
                }
            }
            catch (OperationCanceledException)
            {
                ActiveOutputIndex = 1;
                Output[0].Value = VariantValue.Null;
                Output[1].Value = VariantValue.FromBoolean(true);
                ExecutionLogger.Info("等待信号", "■ 停止信号 → 超时/停止");
                throw;
            }

            SignalCount++;
            ExecutionLogger.Info("等待信号", $"⚡ 收到信号（第 {SignalCount} 次）");

            // 先执行（此时变量仍为 true，走「收到信号」）
            Execute();

            // 再复位，等待下一次触发
            ResetSignalVariable();
        }

        /// <summary>获取实际要监听的变量名（优先绑定的变量名，其次属性值）。</summary>
        private string ResolvedSignalName
        {
            get
            {
                // 优先从绑定中读取变量名
                if (PropertyBindings.TryGetValue("signalVariable", out var binding)
                    && binding.IsBound && !string.IsNullOrEmpty(binding.BoundVariableName))
                    return binding.BoundVariableName;

                return SignalVariable;
            }
        }

        /// <summary>从 GlobalVariableManager 读取信号变量值。</summary>
        private bool EvaluateSignal()
        {
            var name = ResolvedSignalName;
            if (string.IsNullOrEmpty(name))
                return true;

            var variable = GlobalVariableManager?.GetVariable(name);
            return variable?.Value.TryGetBoolean(out var b) == true && b;
        }

        /// <summary>通过 Variable.SetValueAndNotify 设值，UI 自动刷新。</summary>
        private void ResetSignalVariable()
        {
            var name = ResolvedSignalName;
            if (string.IsNullOrEmpty(name)) return;
            var variable = GlobalVariableManager?.GetVariable(name);
            if (variable == null) return;

            variable.SetValueAndNotify(VariantValue.FromBoolean(false));
            ExecutionLogger.Info("等待信号", $"↩ '{name}' → false");
        }

        private void UpdateTitle()
        {
            Title = _executionMode == "循环执行" ? "等待信号 [循环]" : "等待信号 [单次]";
        }
    }
}
