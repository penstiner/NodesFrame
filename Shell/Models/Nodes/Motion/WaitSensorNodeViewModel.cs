using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 等待传感器节点：等待指定传感器触发（示例节点，演示运动控制领域扩展）。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "等待传感器",
        DefaultTitle = "等待传感器",
        Description = "等待指定传感器达到触发条件后输出信号",
        NodeTypeId = "Motion.WaitSensor")]
    [NodeConnector(Title = "传感器ID", Direction = ConnectorDirection.Input,
        ExpectedType = "Int32", Description = "传感器编号")]
    [NodeConnector(Title = "触发信号", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "传感器触发为 true")]
    public class WaitSensorNodeViewModel : NodeViewModel
    {
        public WaitSensorNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel
            {
                Title = "传感器ID",
                ExpectedType = System.TypeCode.Int32
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "触发信号",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        private int _sensorId = 1;
        [NodeProperty(Key = "sensorId")]
        public int SensorId
        {
            get => _sensorId;
            set => SetProperty(ref _sensorId, value);
        }

        private int _timeoutMs = 5000;
        [NodeProperty(Key = "timeoutMs")]
        public int TimeoutMs
        {
            get => _timeoutMs;
            set => SetProperty(ref _timeoutMs, value);
        }

        public override void Execute()
        {
            var idInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var id = idInput.TryGetInt32(out var i) ? i : SensorId;

            // 实际应用中：轮询传感器状态，超时则返回 false
            // 此处演示：模拟传感器触发
            if (Output.Count > 0)
                Output[0].Value = VariantValue.FromBoolean(true);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var idInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var id = idInput.TryGetInt32(out var i) ? i : SensorId;

            // 模拟等待传感器触发（实际应用中轮询硬件）
            await Task.Delay(100, ct);

            if (Output.Count > 0)
                Output[0].Value = VariantValue.FromBoolean(true);
        }
    }
}
