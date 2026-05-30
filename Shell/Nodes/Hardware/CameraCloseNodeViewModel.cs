using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Hardware
{
    [Node(
        Category = "硬件采集",
        DisplayName = "关闭相机",
        DefaultTitle = "关闭相机",
        Description = "关闭相机连接，释放硬件资源",
        NodeTypeId = "Hardware.CameraClose")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "接收上游触发信号")]
    [NodeConnector(Title = "状态", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "关闭是否成功")]
    public class CameraCloseNodeViewModel : NodeViewModel
    {
        public CameraCloseNodeViewModel()
        {
            Title = "关闭相机";

            // 触发输入
            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });

            // 状态输出
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "状态",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        public override void Execute()
        {
            CameraManager.Close();
            Output[0].Value = VariantValue.FromBoolean(true);
        }
    }
}
