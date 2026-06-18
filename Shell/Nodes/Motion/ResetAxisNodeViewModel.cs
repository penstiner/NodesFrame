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
    /// 电机复位节点：支持多轴同时复位，全部轴复位完成后输出完成信号。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "电机复位",
        DefaultTitle = "电机复位",
        Description = "同时复位多个电机轴到原点，全部到位后输出完成信号",
        NodeTypeId = "Motion.ResetAxis")]
    [NodeConnector(Title = "启动", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "上升沿触发：false→true 时启动全部轴复位")]
    [NodeConnector(Title = "运行中", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "任一轴复位中则为 true")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部轴复位到位后为 true")]
    public class ResetAxisNodeViewModel : NodeViewModel
    {
        public ResetAxisNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "启动", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "运行中", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "完成", ExpectedType = TypeCode.Boolean });

            ConfigCollectionHelper.Initialize<AxisParameter, ResetAxisConfig>(
                AxisConfigs,
                () => AxisConfigFactory(),
                out var add, out var remove);
            AddAxisCommand = add;
            RemoveAxisCommand = remove;
        }

        private ResetAxisConfig AxisConfigFactory() =>
            ConfigCollectionHelper.CreateConfig<AxisParameter, ResetAxisConfig>(AxisConfigs, ScheduleRefresh);

        [NodeProperty(Key = "axisConfigs", DisplayName = "轴复位配置", Group = "轴参数")]
        public ObservableCollection<ResetAxisConfig> AxisConfigs { get; set; } = new();
        public ICommand AddAxisCommand { get; }
        public ICommand RemoveAxisCommand { get; }

        private void ScheduleRefresh() => ConfigCollectionHelper.ScheduleRefresh(RefreshAllFilters);
        private void RefreshAllFilters() { foreach (var c in AxisConfigs) c.NotifyFilteredChanged(); }

        private bool _prevStart;
        private bool _homingLaunched;

        public override void Execute()
        {
            var card = CardManager.Card;
            if (card == null || !card.Initialized) { SetOutputs(false, false); return; }

            var startInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var start = startInput.TryGetBoolean(out var b) && b;
            bool risingEdge = start && !_prevStart;
            _prevStart = start;

            if (risingEdge) { LaunchAllHoming(card); _homingLaunched = true; SetOutputs(true, false); }
            else if (_homingLaunched)
            {
                if (AllAxesDone(card)) { _homingLaunched = false; SetOutputs(false, true); }
                else { SetOutputs(true, false); }
            }
            else { SetOutputs(false, false); }
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var card = CardManager.Card;
            if (card == null || !card.Initialized) { SetOutputs(false, false); return; }
            var startInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var start = startInput.TryGetBoolean(out var b) && b;
            bool risingEdge = start && !_prevStart;
            _prevStart = start;
            if (risingEdge) { var tasks = AxisConfigs.Select(cfg => card.ProcessHomeMove(cfg.AxisId, cfg.Speed, 0)); SetOutputs(true, false); try { await Task.WhenAll(tasks); } catch (OperationCanceledException) { SetOutputs(false, false); return; } SetOutputs(false, true); }
            else { SetOutputs(false, false); }
        }

        private void LaunchAllHoming(IControlCard card) { foreach (var cfg in AxisConfigs) _ = card.ProcessHomeMove(cfg.AxisId, cfg.Speed, 0); }
        private bool AllAxesDone(IControlCard card) { foreach (var cfg in AxisConfigs) { if (!card.GetAxisStatus(cfg.AxisId)) return false; } return true; }
        private void SetOutputs(bool running, bool completed) { if (Output.Count > 0) Output[0].Value = VariantValue.FromBoolean(running); if (Output.Count > 1) Output[1].Value = VariantValue.FromBoolean(completed); }
    }
}
