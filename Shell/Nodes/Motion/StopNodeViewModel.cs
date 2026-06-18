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
    /// 急停节点：对指定轴执行减速停止。支持多轴同时停止。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "停止轴",
        DefaultTitle = "停止轴",
        Description = "对选定的轴执行减速停止",
        NodeTypeId = "Motion.Stop")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时执行急停")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部停止成功为 true")]
    public class StopNodeViewModel : MotionNodeBase
    {
        public StopNodeViewModel()
        {
            ConfigCollectionHelper.Initialize<AxisParameter, StopConfig>(
                AxisConfigs,
                () => ConfigCollectionHelper.CreateConfig<AxisParameter, StopConfig>(AxisConfigs, () => { }),
                out var add, out var remove);
            AddAxisCommand = add;
            RemoveAxisCommand = remove;
        }

        [NodeProperty(Key = "axisConfigs", DisplayName = "急停轴列表", Group = "轴参数")]
        public ObservableCollection<StopConfig> AxisConfigs { get; set; } = new();
        public ICommand AddAxisCommand { get; }
        public ICommand RemoveAxisCommand { get; }

        public override void Execute()
        {
            if (!GetInputBool()) ExecutionLogger.Warning("停止轴", "等待触发信号...");
            if (Card == null) ExecutionLogger.Warning("停止轴", "等待控制卡就绪...");
            if (AxisConfigs.Count == 0) ExecutionLogger.Warning("停止轴", "等待轴配置...");
            while (!GetInputBool() || Card == null || AxisConfigs.Count == 0)
                Thread.Sleep(100);

            bool allOk = true;
            foreach (var cfg in AxisConfigs)
            {
                if (!Card.Stop(cfg.AxisId))
                    allOk = false;
            }
            SetOutputBool(allOk);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (!GetInputBool()) ExecutionLogger.Warning("停止轴", "等待触发信号...");
            if (Card == null) ExecutionLogger.Warning("停止轴", "等待控制卡就绪...");
            if (AxisConfigs.Count == 0) ExecutionLogger.Warning("停止轴", "等待轴配置...");
            while (!GetInputBool() || Card == null || AxisConfigs.Count == 0)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            }

            bool allOk = await Task.Run(() =>
            {
                bool ok = true;
                foreach (var cfg in AxisConfigs)
                    if (!Card.Stop(cfg.AxisId)) ok = false;
                return ok;
            }, ct);
            SetOutputBool(allOk);
        }
    }
}
