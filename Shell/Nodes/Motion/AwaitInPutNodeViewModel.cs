using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hardware.Card.Interface;
using Hardware.Card.Models;
using Nodify;
using Shell.Models.Attributes;

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
    public class AwaitInPutNodeViewModel : MotionNodeBase
    {
        public AwaitInPutNodeViewModel()
        {
            SignalConfigs.Add(NewSignalConfig());

            AddSignalCommand = new DelegateCommand(() => SignalConfigs.Add(NewSignalConfig()));
            RemoveSignalCommand = new DelegateCommand<SignalConfig>(cfg =>
            {
                if (cfg != null) { SignalConfigs.Remove(cfg); RefreshAllFilters(); }
            });

            // 集合变化时刷新所有行的过滤列表
            SignalConfigs.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (SignalConfig item in e.NewItems) item.Siblings = SignalConfigs;
                RefreshAllFilters();
            };
        }

        private SignalConfig NewSignalConfig()
        {
            var cfg = new SignalConfig { Siblings = SignalConfigs };
            cfg.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SignalConfig.RegId))
                    ScheduleRefresh();
            };
            return cfg;
        }

        /// <summary>延迟刷新：避免在 PropertyChanged 回调链内同步修改 ItemsSource 导致重入 / 栈溢出。</summary>
        private void ScheduleRefresh()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(RefreshAllFilters));
        }

        private void RefreshAllFilters()
        {
            foreach (var cfg in SignalConfigs) cfg.NotifyFilteredChanged();
        }

        [NodeProperty(Key = "signalConfigs", DisplayName = "信号配置列表", Group = "信号参数")]
        public ObservableCollection<SignalConfig> SignalConfigs { get; set; } = new();

        public ICommand AddSignalCommand { get; }
        public ICommand RemoveSignalCommand { get; }

        public override void Execute()
        {
            var card = Card;
            if (card == null || !card.Initialized) return;
            if (!GetInputBool()) return;
            if (SignalConfigs.Count == 0) return;

            while (!AllSignalsActive(card))
            {
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
