using System.Linq;
using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 电机运动节点：控制电机按指定参数运动（示例节点，演示运动控制领域扩展）。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "电机运动",
        DefaultTitle = "电机运动",
        Description = "控制电机按指定位置、速度、加速度运动",
        NodeTypeId = "Motion.MotorMove")]
    [NodeConnector(Title = "目标位置", Direction = ConnectorDirection.Input,
        ExpectedType = "Double", Description = "目标位置 (mm)")]
    [NodeConnector(Title = "当前速度", Direction = ConnectorDirection.Input,
        ExpectedType = "Double", Description = "运动速度 (mm/s)")]
    [NodeConnector(Title = "完成信号", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "运动完成则为 true")]
    public class MotorMoveNodeViewModel : NodeViewModel
    {
        public MotorMoveNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel
            {
                Title = "目标位置",
                ExpectedType = System.TypeCode.Double
            });
            AddInputConnector(new ConnectorViewModel
            {
                Title = "当前速度",
                ExpectedType = System.TypeCode.Double
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "完成信号",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        private double _targetPosition;
        [NodeProperty(Key = "targetPosition")]
        public double TargetPosition
        {
            get => _targetPosition;
            set => SetProperty(ref _targetPosition, value);
        }

        private double _speed = 100.0;
        [NodeProperty(Key = "speed")]
        public double Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        private double _acceleration = 500.0;
        [NodeProperty(Key = "acceleration")]
        public double Acceleration
        {
            get => _acceleration;
            set => SetProperty(ref _acceleration, value);
        }

        public override void Execute()
        {
            var posInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var speedInput = Input.ElementAtOrDefault(1)?.Value ?? VariantValue.Null;

            var pos = posInput.TryGetDouble(out var p) ? p : TargetPosition;
            var speed = speedInput.TryGetDouble(out var s) ? s : Speed;

            // 实际应用中：发送运动指令到电机控制器
            // 此处演示：模拟运动完成
            if (Output.Count > 0)
                Output[0].Value = VariantValue.FromBoolean(true);
        }
    }
}
