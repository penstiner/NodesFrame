using System.Threading;
using System.Threading.Tasks;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 控制卡关闭节点：关闭运动控制卡硬件，释放资源。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "控制卡关闭",
        DefaultTitle = "控制卡关闭",
        Description = "关闭运动控制卡，释放硬件资源",
        NodeTypeId = "Motion.ControlCardClose")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时执行关闭")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "关闭成功为 true")]
    public class ControlCardCloseNodeViewModel : MotionNodeBase
    {
        public override void Execute()
        {
            if (!GetInputBool()) return;
            if (Card == null) return;

            bool ok = Card.Close();
            if (ok)
            {
                SetOutputBool(true);
                CardManager.Unregister();
                return;
            }

            State = ExecutionState.Error;
            return;
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (!GetInputBool()) return;
            if (Card == null) return;

            bool ok = Card.Close();
            if (ok)
            {
                SetOutputBool(true);
                CardManager.Unregister();
                return;
            }

            State = ExecutionState.Error;
            return;
        }
    }
}
