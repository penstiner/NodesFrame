using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Flow
{
    [Node(
        Category = "流程控制",
        DisplayName = "流程结束",
        DefaultTitle = "流程结束",
        Description = "流程的终止节点，标记执行结束",
        NodeTypeId = "Flow.End")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "接收上游完成信号")]
    public class FlowEndNodeViewModel : NodeViewModel
    {
        public FlowEndNodeViewModel()
        {
            Title = "流程结束";

            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        public override void Execute()
        {
            // 流程结束，无操作
        }
    }
}
