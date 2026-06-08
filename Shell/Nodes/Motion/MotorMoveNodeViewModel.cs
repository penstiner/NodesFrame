using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hardware.Card.Interface;
using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 单轴定位节点：驱动单个轴到目标位置，阻塞等待到位/报警/超时后输出完成信号。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "单轴定位",
        DefaultTitle = "单轴定位",
        Description = "驱动单个轴按绝对/相对方式运动到目标位置，到位后输出完成信号",
        NodeTypeId = "Motion.MotorMove")]
    [NodeConnector(Title = "启动", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时启动运动并阻塞等待到位")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "轴到位后为 true，超时/报警为 false")]
    public class MotorMoveNodeViewModel : MotionNodeBase
    {
        private int _axisId;
        [NodeProperty(Key = "axisId", DisplayName = "轴号", Group = "轴参数", BindableToVariable = false)]
        public int AxisId
        {
            get => _axisId;
            set => SetProperty(ref _axisId, value);
        }

        private int _moveType;
        [NodeProperty(Key = "moveType", DisplayName = "定位方式", Group = "轴参数",
            Options = "绝对定位,相对定位", BindableToVariable = false)]
        public int MoveType
        {
            get => _moveType;
            set => SetProperty(ref _moveType, value);
        }

        private double _speed = 50;
        [NodeProperty(Key = "speed", DisplayName = "默认速度 (mm/s)", Group = "运动参数")]
        public double Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        private double _position;
        [NodeProperty(Key = "position", DisplayName = "默认位置 (mm)", Group = "运动参数")]
        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        private double _timeout = 30000;
        [NodeProperty(Key = "timeout", DisplayName = "超时时间 (ms)", Group = "运动参数", Min = 100, Max = 300000)]
        public double Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }

        public override void Execute()
        {
            var card = Card;
            if (card == null || !card.Initialized) return;
            if (!GetInputBool()) return;

            if (!DoMove(card))
            {
                State = ExecutionState.Error;
                return;
            }

            var sw = Stopwatch.StartNew();
            while (!card.GetAxisStatus(AxisId))
            {
                if (card.GetAlarmValue(AxisId))
                {
                    card.Stop(AxisId);
                    State = ExecutionState.Error;
                    return;
                }
                if (sw.Elapsed.TotalMilliseconds > Timeout)
                {
                    card.Stop(AxisId);
                    State = ExecutionState.Error;
                    return;
                }
                Thread.Sleep(20);
            }

            SetOutputBool(true);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var card = Card;
            if (card == null || !card.Initialized) return;
            if (!GetInputBool()) return;

            SetOutputBool(false);

            if (!DoMove(card))
            {
                State = ExecutionState.Error;
                return;
            }

            var sw = Stopwatch.StartNew();
            while (!card.GetAxisStatus(AxisId))
            {
                ct.ThrowIfCancellationRequested();

                if (card.GetAlarmValue(AxisId))
                {
                    card.Stop(AxisId);
                    State = ExecutionState.Error;
                    return;
                }
                if (sw.Elapsed.TotalMilliseconds > Timeout)
                {
                    card.Stop(AxisId);
                    State = ExecutionState.Error;
                    return;
                }
                await Task.Delay(20, ct);
            }

            SetOutputBool(true);
        }

        private bool DoMove(IControlCard card)
        {
            return MoveType == 1
                ? card.RelMove(AxisId, Speed, Position)
                : card.AbsMove(AxisId, Speed, Position);
        }
    }
}
