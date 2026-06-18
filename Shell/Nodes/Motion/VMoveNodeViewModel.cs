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
    /// 连续运动节点：对指定轴启动匀速连续运动（VMove）。不自动停止，需配合急停节点使用。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "连续运动",
        DefaultTitle = "连续运动",
        Description = "启动指定轴的匀速连续运动（不停），需配合急停节点停止",
        NodeTypeId = "Motion.VMove")]
    [NodeConnector(Title = "启动", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时启动全部轴的连续运动")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部启动成功为 true")]
    public class VMoveNodeViewModel : MotionNodeBase
    {
        public VMoveNodeViewModel()
        {
            ConfigCollectionHelper.Initialize<AxisParameter, VMoveConfig>(
                AxisConfigs,
                () => ConfigCollectionHelper.CreateConfig<AxisParameter, VMoveConfig>(AxisConfigs, () => { }),
                out var add, out var remove);
            AddAxisCommand = add;
            RemoveAxisCommand = remove;
        }

        [NodeProperty(Key = "axisConfigs", DisplayName = "运动轴列表", Group = "轴参数")]
        public ObservableCollection<VMoveConfig> AxisConfigs { get; set; } = new();
        public ICommand AddAxisCommand { get; }
        public ICommand RemoveAxisCommand { get; }

        public override void Execute()
        {
            if (!GetInputBool()) return;
            if (Card == null) return;
            if (AxisConfigs.Count == 0) return;

            bool allOk = true;
            foreach (var cfg in AxisConfigs)
            {
                if (!Card.VMove(cfg.AxisId, cfg.Speed))
                    allOk = false;
            }
            SetOutputBool(allOk);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (!GetInputBool()) return;
            if (Card == null) return;
            if (AxisConfigs.Count == 0) return;

            bool allOk = await Task.Run(() =>
            {
                bool ok = true;
                foreach (var cfg in AxisConfigs)
                    if (!Card.VMove(cfg.AxisId, cfg.Speed)) ok = false;
                return ok;
            }, ct);
            SetOutputBool(allOk);
        }
    }
}
