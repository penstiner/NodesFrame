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
    [Node(
        Category = "运动控制",
        DisplayName = "等待信号",
        DefaultTitle = "等待信号",
        Description = "等待指定的输入信号全部就绪后放行",
        NodeTypeId = "Motion.AwaitInput")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时开始等待信号")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部信号就绪后为 true")]
    public class AwaitInputNodeViewModel : MotionNodeBase
    {
        public AwaitInputNodeViewModel()
        {
            ConfigCollectionHelper.Initialize<IOParameter, SignalConfig>(
                SignalConfigs,
                () => SignalConfigFactory(),
                out var add, out var remove);
            AddSignalCommand = add;
            RemoveSignalCommand = remove;
        }

        private SignalConfig SignalConfigFactory() =>
            ConfigCollectionHelper.CreateConfig<IOParameter, SignalConfig>(SignalConfigs, ScheduleRefresh);

        [NodeProperty(Key = "signalConfigs", DisplayName = "信号配置列表", Group = "信号参数")]
        public ObservableCollection<SignalConfig> SignalConfigs { get; set; } = new();
        public ICommand AddSignalCommand { get; }
        public ICommand RemoveSignalCommand { get; }

        private void ScheduleRefresh() => ConfigCollectionHelper.ScheduleRefresh(RefreshAllFilters);
        private void RefreshAllFilters() { foreach (var c in SignalConfigs) c.NotifyFilteredChanged(); }

        public override void Execute()
        {
            var card = Card;
            if (card == null || !card.Initialized) return;
            if (!GetInputBool()) return;
            if (SignalConfigs.Count == 0) return;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (!AllSignalsActive(card))
            {
                if (sw.ElapsedMilliseconds > 30000) // 30s 超时保护
                {
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
            if (SignalConfigs.Count == 0) return;

            while (!AllSignalsActive(card))
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(20, ct);
            }

            SetOutputBool(true);
        }

        private bool AllSignalsActive(IControlCard card)
        {
            foreach (var cfg in SignalConfigs)
            {
                bool isOn = card.ReadIn(cfg.RegId);
                // IO_STATUS.ON = 低电平有效 → 信号为低时满足
                if (cfg.Condition == (int)IO_STATUS.ON)
                    isOn = !isOn;
                if (!isOn) return false;
            }
            return true;
        }
    }
}
