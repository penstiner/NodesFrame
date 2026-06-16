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
            if (card == null || !card.Initialized) { ExecutionLogger.Warning("等待信号", "等待控制卡就绪..."); }
            else if (!GetInputBool()) { ExecutionLogger.Warning("等待信号", "等待触发信号..."); }
            else if (SignalConfigs.Count == 0) { ExecutionLogger.Warning("等待信号", "等待信号配置..."); }
            while (card == null || !card.Initialized || !GetInputBool() || SignalConfigs.Count == 0)
            {
                Thread.Sleep(100);
                card = Card;
            }

            ExecutionLogger.Info("等待信号", "开始等待信号就绪...");
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
            if (card == null || !card.Initialized) { ExecutionLogger.Warning("等待信号", "等待控制卡就绪..."); }
            else if (!GetInputBool()) { ExecutionLogger.Warning("等待信号", "等待触发信号..."); }
            else if (SignalConfigs.Count == 0) { ExecutionLogger.Warning("等待信号", "等待信号配置..."); }
            while (card == null || !card.Initialized || !GetInputBool() || SignalConfigs.Count == 0)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
                card = Card;
            }

            ExecutionLogger.Info("等待信号", "开始等待信号就绪...");
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
                bool hwIsOn = card.ReadIn(cfg.RegId);                    // true=硬件ON(低电平), false=硬件OFF(高电平)
                bool expectedMatches = (cfg.Condition == (int)IO_STATUS.ON) ? hwIsOn : !hwIsOn;
                if (!expectedMatches) return false;                       // 任一信号不满足 → 整体不通过
            }
            return true;                                                  // 全部信号满足 → 放行
        }
    }
}
