using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Flow
{
    [Node(
        Category = "流程控制",
        DisplayName = "流程开始",
        DefaultTitle = "流程开始",
        Description = "流程的起始节点，标记执行入口",
        NodeTypeId = "Flow.Start")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "执行触发信号")]
    public class FlowStartNodeViewModel : NodeViewModel
    {
        public FlowStartNodeViewModel()
        {
            Title = "流程开始";

            AddOutputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        public override void Execute()
        {
            Output[0].Value = VariantValue.FromBoolean(true);
        }
    }
}
