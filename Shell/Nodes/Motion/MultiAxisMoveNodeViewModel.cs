using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hardware.Card.Interface;
using Nodify;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 多轴并行定位节点：同时启动多个轴运动，等待全部轴到位后输出完成信号。
    /// 轴配置通过 DataGrid 在编辑弹窗中添加/删除行。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "多轴定位",
        DefaultTitle = "多轴定位",
        Description = "同时驱动多个轴到目标位置，全部到位后输出完成信号",
        NodeTypeId = "Motion.MultiAxisMove")]
    [NodeConnector(Title = "启动", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "上升沿触发：false→true 时启动全部轴")]
    [NodeConnector(Title = "运行中", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "任一轴运动中则为 true")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部轴到位后为 true")]
    public class MultiAxisMoveNodeViewModel : NodeViewModel
    {
        public MultiAxisMoveNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "启动", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "运行中", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "完成", ExpectedType = TypeCode.Boolean });

            AxisConfigs.Add(new AxisConfig { AxisId = 0, Speed = 50, Position = 100 });
            AxisConfigs.Add(new AxisConfig { AxisId = 1, Speed = 80, Position = 200 });

            AddAxisCommand = new DelegateCommand(() =>
                AxisConfigs.Add(new AxisConfig { AxisId = AxisConfigs.Count, Speed = 50, Position = 0 }));
            RemoveAxisCommand = new DelegateCommand<AxisConfig>(cfg =>
            { if (cfg != null) AxisConfigs.Remove(cfg); });
        }

        [NodeProperty(Key = "axisConfigs", DisplayName = "轴配置列表", Group = "轴参数")]
        public ObservableCollection<AxisConfig> AxisConfigs { get; set; } = new();

        public ICommand AddAxisCommand { get; }
        public ICommand RemoveAxisCommand { get; }

        // ── 定位方式 ──
        private int _moveType;
        [NodeProperty(Key = "moveType", DisplayName = "定位方式",
            Options = "绝对定位,相对定位")]
        public int MoveType
        {
            get => _moveType;
            set => SetProperty(ref _moveType, value);
        }

        // ── 内部状态 ──
        private bool _prevStart;
        private bool _motionLaunched;
        private int _launchedCount;

        public override void Execute()
        {
            var card = CardManager.Card;
            if (card == null || !card.Initialized)
            {
                SetOutputs(false, false);
                return;
            }

            var startInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var start = startInput.TryGetBoolean(out var b) && b;
            bool risingEdge = start && !_prevStart;
            _prevStart = start;

            if (risingEdge)
            {
                LaunchAllAxes(card);
                _motionLaunched = true;
                SetOutputs(true, false);
            }
            else if (_motionLaunched)
            {
                if (AllAxesDone(card))
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
                LaunchAllAxes(card);
                SetOutputs(true, false);

                while (!AllAxesDone(card))
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(20, ct);
                }
                SetOutputs(false, true);
            }
            else { SetOutputs(false, false); }
        }

        private void LaunchAllAxes(IControlCard card)
        {
            _launchedCount = 0;
            foreach (var cfg in AxisConfigs)
            {
                bool ok = MoveType == 1
                    ? card.RelMove(cfg.AxisId, cfg.Speed, cfg.Position)
                    : card.AbsMove(cfg.AxisId, cfg.Speed, cfg.Position);
                if (ok) _launchedCount++;
            }
        }

        private bool AllAxesDone(IControlCard card)
        {
            if (_launchedCount == 0) return true;
            foreach (var cfg in AxisConfigs)
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


