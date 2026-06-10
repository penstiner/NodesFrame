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
            ConfigCollectionHelper.Initialize<IOParameter, OutputSignalConfig>(
                SignalConfigs,
                () => SignalConfigFactory(),
                out var add, out var remove);
            AddSignalCommand = add;
            RemoveSignalCommand = remove;
        }

        private OutputSignalConfig SignalConfigFactory() =>
            ConfigCollectionHelper.CreateConfig<IOParameter, OutputSignalConfig>(SignalConfigs, ScheduleRefresh);

        [NodeProperty(Key = "signalConfigs", DisplayName = "信号配置列表", Group = "信号参数")]
        public ObservableCollection<OutputSignalConfig> SignalConfigs { get; set; } = new();
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
