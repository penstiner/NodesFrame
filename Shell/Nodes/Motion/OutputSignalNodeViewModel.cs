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

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 输出信号节点：根据配置批量设置硬件输出信号为 ON / OFF。
    /// 触发 → 全部输出写入目标电平 → 结果
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "输出信号",
        DefaultTitle = "输出信号",
        Description = "批量设置控制卡输出信号的电平状态（ON/OFF）",
        NodeTypeId = "Motion.OutputSignal")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时执行写入")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部写入成功为 true")]
    public class OutputSignalNodeViewModel : MotionNodeBase
    {
        public OutputSignalNodeViewModel()
        {
            SignalConfigs.Add(NewSignalConfig());

            AddSignalCommand = new DelegateCommand(() => SignalConfigs.Add(NewSignalConfig()));
            RemoveSignalCommand = new DelegateCommand<OutputSignalConfig>(cfg =>
            {
                if (cfg != null) { SignalConfigs.Remove(cfg); RefreshAllFilters(); }
            });

            SignalConfigs.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (OutputSignalConfig item in e.NewItems) item.Siblings = SignalConfigs;
                RefreshAllFilters();
            };
        }

        private OutputSignalConfig NewSignalConfig()
        {
            var cfg = new OutputSignalConfig { Siblings = SignalConfigs };
            cfg.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OutputSignalConfig.RegId))
                    ScheduleRefresh();
            };
            return cfg;
        }

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
        public ObservableCollection<OutputSignalConfig> SignalConfigs { get; set; } = new();

        public ICommand AddSignalCommand { get; }
        public ICommand RemoveSignalCommand { get; }

        public override void Execute()
        {
            var card = Card;
            if (card == null || !card.Initialized) return;
            if (!GetInputBool()) return;
            if (SignalConfigs.Count == 0) return;

            bool allOk = WriteAllOutputs(card);
            SetOutputBool(allOk);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var card = Card;
            if (card == null || !card.Initialized) return;
            if (!GetInputBool()) return;
            if (SignalConfigs.Count == 0) return;

            ct.ThrowIfCancellationRequested();

            bool allOk = await Task.Run(() => WriteAllOutputs(card), ct);
            SetOutputBool(allOk);
        }

        /// <summary>批量写入全部输出信号，返回是否全部成功。</summary>
        private bool WriteAllOutputs(IControlCard card)
        {
            bool allOk = true;
            foreach (var cfg in SignalConfigs)
            {
                var target = cfg.TargetState == 1 ? IO_STATUS.ON : IO_STATUS.OFF;
                bool ok = card.WriteState(cfg.RegId, target);
                if (!ok) allOk = false;
            }
            return allOk;
        }
    }
}
