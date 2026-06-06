using Shell.Models;
using Shell.Models.Attributes;
using Shell.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 控制卡初始化节点：初始化运动控制卡硬件。
    /// 初始化失败时节点进入 Error 状态并阻塞等待，不向下游输出；
    /// 仅可通过流程停止/外部取消退出。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "控制卡初始化",
        DefaultTitle = "控制卡初始化",
        Description = "初始化运动控制卡，失败时阻塞并标记 Error",
        NodeTypeId = "Motion.ControlCardInit")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时执行初始化")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "初始化成功为 true")]
    public class ControlCardInitViewModel : NodeViewModel
    {
        public ControlCardInitViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "触发", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "结果", ExpectedType = TypeCode.Boolean });
        }

        public override void Execute()
        {
            // 检查触发信号
            var triggerInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            if (!(triggerInput.TryGetBoolean(out var b) && b)) return;

            var card = CardManager.Card;
            if (card == null) return;

            bool ok = card.Init();
            if (ok)
            {
                SetOutput(true);
                return;
            }

            // 初始化失败 → Error 状态，永久阻塞
            State = ExecutionState.Error;
            while (true) Thread.Sleep(100);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var triggerInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            if (!(triggerInput.TryGetBoolean(out var b) && b)) return;

            var card = CardManager.Card;
            if (card == null) return;

            bool ok = card.Init();
            if (ok)
            {
                SetOutput(true);
                return;
            }

            // 初始化失败 → Error 状态，阻塞直到外部取消
            State = ExecutionState.Error;
            await Task.Delay(Timeout.Infinite, ct);
        }

        private void SetOutput(bool value)
        {
            if (Output.Count > 0) Output[0].Value = VariantValue.FromBoolean(value);
        }
    }
}
