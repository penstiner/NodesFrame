using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hardware.Card.Models;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 控制卡初始化节点：初始化硬件 → 批量配置轴参数（脉冲模式/限位逻辑/伺服使能）。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "控制卡初始化",
        DefaultTitle = "控制卡初始化",
        Description = "初始化运动控制卡并配置轴参数，失败时阻塞",
        NodeTypeId = "Motion.ControlCardInit")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时执行初始化")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "初始化成功为 true")]
    public class ControlCardInitNodeViewModel : MotionNodeBase
    {
        public ControlCardInitNodeViewModel()
        {
            ConfigCollectionHelper.Initialize<AxisParameter, AxisInitConfig>(
                AxisConfigs,
                () => ConfigCollectionHelper.CreateConfig<AxisParameter, AxisInitConfig>(AxisConfigs, ScheduleRefresh),
                out var add, out var remove);
            AddAxisCommand = add;
            RemoveAxisCommand = remove;
        }

        [NodeProperty(Key = "axisConfigs", DisplayName = "轴参数配置", Group = "参数设置")]
        public ObservableCollection<AxisInitConfig> AxisConfigs { get; set; } = new();
        public ICommand AddAxisCommand { get; }
        public ICommand RemoveAxisCommand { get; }

        private void ScheduleRefresh() => ConfigCollectionHelper.ScheduleRefresh(RefreshAllFilters);
        private void RefreshAllFilters() { foreach (var c in AxisConfigs) c.NotifyFilteredChanged(); }

        public override void Execute()
        {
            if (!GetInputBool()) ExecutionLogger.Warning("控制卡初始化", "等待触发信号...");
            if (Card == null) ExecutionLogger.Warning("控制卡初始化", "等待控制卡注册...");
            while (!GetInputBool() || Card == null)
                Thread.Sleep(100);

            bool ok = Card!.Init();
            if (ok)
            {
                ApplyAxisConfigs();
                SetOutputBool(true);
                return;
            }

            State = ExecutionState.Error;
            while (true) Thread.Sleep(100);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (!GetInputBool()) ExecutionLogger.Warning("控制卡初始化", "等待触发信号...");
            if (Card == null) ExecutionLogger.Warning("控制卡初始化", "等待控制卡注册...");
            while (!GetInputBool() || Card == null)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            }

            bool ok = Card!.Init();
            if (ok)
            {
                await Task.Run(() => ApplyAxisConfigs(), ct);
                SetOutputBool(true);
                return;
            }

            State = ExecutionState.Error;
            await Task.Delay(Timeout.Infinite, ct);
        }

        private void ApplyAxisConfigs()
        {
            foreach (var cfg in AxisConfigs)
            {
                Card?.SetPulseMode(cfg.AxisId, (ushort)cfg.PulseMode);
                Card?.SetLimitMode(cfg.AxisId, (ushort)cfg.OrgLogic);
                Card?.SetLimitMode(cfg.AxisId, (ushort)cfg.PelLogic);
                Card?.SetLimitMode(cfg.AxisId, (ushort)cfg.NelLogic);
                if (cfg.EnableServo)
                    Card?.SetServoPower(cfg.AxisId, 0);
            }
        }
    }
}
