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
    /// 限位传感器检测节点：检查指定轴的 ORG/PEL/NEL 传感器状态，全部匹配期望值后输出 true。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "轴IO检测",
        DefaultTitle = "轴IO检测",
        Description = "检测指定轴的 ORG/PEL/NEL 传感器状态，全部匹配时输出 true",
        NodeTypeId = "Motion.SensorCheck")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时读取传感器")]
    [NodeConnector(Title = "结果", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "全部传感器状态匹配期望值则为 true")]
    public class SensorCheckNodeViewModel : MotionNodeBase
    {
        public SensorCheckNodeViewModel()
        {
            ConfigCollectionHelper.Initialize<AxisParameter, SensorCheckConfig>(
                SensorConfigs,
                () => ConfigCollectionHelper.CreateConfig<AxisParameter, SensorCheckConfig>(SensorConfigs, () => { }),
                out var add, out var remove);
            AddSensorCommand = add;
            RemoveSensorCommand = remove;
        }

        [NodeProperty(Key = "sensorConfigs", DisplayName = "传感器配置", Group = "检测参数")]
        public ObservableCollection<SensorCheckConfig> SensorConfigs { get; set; } = new();
        public ICommand AddSensorCommand { get; }
        public ICommand RemoveSensorCommand { get; }

        public override void Execute()
        {
            if (!GetInputBool()) return;
            if (Card == null) return;
            if (SensorConfigs.Count == 0) return;

            bool allMatch = SensorConfigs.All(cfg =>
            {
                bool actual = cfg.SensorType switch
                {
                    0 => Card.GetORGStatus(cfg.AxisId),
                    1 => Card.GetPEL(cfg.AxisId),
                    2 => Card.GetNEL(cfg.AxisId),
                    _ => false
                };
                bool expected = cfg.ExpectedState == 0; // 0=已触发
                return actual == expected;
            });

            SetOutputBool(allMatch);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            if (!GetInputBool()) return;
            if (Card == null) return;
            if (SensorConfigs.Count == 0) return;

            bool result = await Task.Run(() =>
                SensorConfigs.All(cfg =>
                {
                    bool actual = cfg.SensorType switch
                    {
                        0 => Card.GetORGStatus(cfg.AxisId),
                        1 => Card.GetPEL(cfg.AxisId),
                        2 => Card.GetNEL(cfg.AxisId),
                        _ => false
                    };
                    return actual == (cfg.ExpectedState == 0);
                }), ct);

            SetOutputBool(result);
        }
    }
}
