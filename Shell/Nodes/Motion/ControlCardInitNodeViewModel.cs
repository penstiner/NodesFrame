using System.Threading;
using System.Threading.Tasks;
using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 控制卡初始化节点：初始化运动控制卡硬件。
    /// 初始化失败时节点进入 Error 状态并阻塞等待，不向下游输出。
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
    public class ControlCardInitNodeViewModel : MotionNodeBase
    {
        public override void Execute()
        {
            if (!GetInputBool()) return;
            if (Card == null) return;

            bool ok = Card.Init();
            if (ok)
            {
                SetOutputBool(true);
                return;
            }

            State = ExecutionState.Error;
            while (true) Thread.Sleep(100);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (!GetInputBool()) return;
            if (Card == null) return;

            bool ok = Card.Init();
            if (ok)
            {
                SetOutputBool(true);
                return;
            }

            State = ExecutionState.Error;
            await Task.Delay(Timeout.Infinite, ct);
        }
    }
}
