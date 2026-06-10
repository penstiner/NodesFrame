using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hardware.Card.Interface;
using Hardware.Card.Models;
using Nodify;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 电机运动节点：下拉选轴 → 配置定位方式/速度/位置 → 同时启动全部轴运动。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "轴运动",
        DefaultTitle = "轴运动",
        Description = "驱动一个或多个轴按绝对/相对方式运动到目标位置，全部到位后输出完成信号",
        NodeTypeId = "Motion.MotorMove")]
    [NodeConnector(Title = "启动", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "上升沿触发：false→true 时启动全部轴")]
    [NodeConnector(Title = "运行中", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "任一轴运动中则为 true")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部轴到位后为 true")]
    public class MotorMoveNodeViewModel : NodeViewModel
    {
        public MotorMoveNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "启动", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "运行中", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "完成", ExpectedType = TypeCode.Boolean });

            ConfigCollectionHelper.Initialize<AxisParameter, MotorMoveConfig>(
                Configs,
                () => ConfigFactory(),
                out var add, out var remove);
            AddConfigCommand = add;
            RemoveConfigCommand = remove;
        }

        private MotorMoveConfig ConfigFactory() =>
            ConfigCollectionHelper.CreateConfig<AxisParameter, MotorMoveConfig>(Configs, ScheduleRefresh);

        [NodeProperty(Key = "configs", DisplayName = "轴配置列表", Group = "轴参数")]
        public ObservableCollection<MotorMoveConfig> Configs { get; set; } = new();
        public ICommand AddConfigCommand { get; }
        public ICommand RemoveConfigCommand { get; }

        private void ScheduleRefresh() => ConfigCollectionHelper.ScheduleRefresh(RefreshAllFilters);
        private void RefreshAllFilters() { foreach (var c in Configs) c.NotifyFilteredChanged(); }

        private bool _prevStart;
        private bool _motionLaunched;

        public override void Execute()
        {
            var card = CardManager.Card;
            if (card == null || !card.Initialized) { SetOutputs(false, false); return; }

            var startInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var start = startInput.TryGetBoolean(out var b) && b;
            bool risingEdge = start && !_prevStart;
            _prevStart = start;

            if (risingEdge)
            {
                LaunchAll(card);
                _motionLaunched = true;
                SetOutputs(true, false);
            }
            else if (_motionLaunched)
            {
                if (AllDone(card))
                {
                    _motionLaunched = false;
                    SetOutputs(false, true);
                }
                else
                {
                    SetOutputs(true, false);
                }
            }
            else
            {
                SetOutputs(false, false);
            }
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var card = CardManager.Card;
            if (card == null || !card.Initialized) { SetOutputs(false, false); return; }

            var startInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var start = startInput.TryGetBoolean(out var b) && b;
            bool risingEdge = start && !_prevStart;
            _prevStart = start;

            if (risingEdge)
            {
                LaunchAll(card);
                SetOutputs(true, false);

                while (!AllDone(card))
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(20, ct);
                }
                SetOutputs(false, true);
            }
            else { SetOutputs(false, false); }
        }

        private void LaunchAll(IControlCard card)
        {
            foreach (var cfg in Configs)
            {
                _ = cfg.MoveType == 1
                    ? card.RelMove(cfg.AxisId, cfg.Speed, cfg.Position)
                    : card.AbsMove(cfg.AxisId, cfg.Speed, cfg.Position);
            }
        }

        private bool AllDone(IControlCard card)
        {
            foreach (var cfg in Configs)
            {
                if (!card.GetAxisStatus(cfg.AxisId))
                    return false;
            }
            return true;
        }

        private void SetOutputs(bool running, bool completed)
        {
            if (Output.Count > 0) Output[0].Value = VariantValue.FromBoolean(running);
            if (Output.Count > 1) Output[1].Value = VariantValue.FromBoolean(completed);
        }
    }
}
