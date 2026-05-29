using System.Collections.Generic;
using System.Linq;
using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 运动序列节点：将多个运动指令编排为序列执行（示例节点，演示运动控制领域扩展）。
    /// </summary>
    [Node(
        Category = "运动控制",
        DisplayName = "运动序列",
        DefaultTitle = "运动序列",
        Description = "编排多个运动指令按顺序执行，完成后输出完成信号",
        NodeTypeId = "Motion.MotionSequence")]
    [NodeConnector(Title = "启动指令", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "true 时启动序列")]
    [NodeConnector(Title = "序列中...", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "序列执行过程中为 true")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "整个序列完成时为 true")]
    public class MotionSequenceNodeViewModel : NodeViewModel
    {
        public MotionSequenceNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel
            {
                Title = "启动指令",
                ExpectedType = System.TypeCode.Boolean
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "序列中...",
                ExpectedType = System.TypeCode.Boolean
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "完成",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        private int _currentStep;
        public int CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        private int _totalSteps = 3;
        [NodeProperty(Key = "totalSteps")]
        public int TotalSteps
        {
            get => _totalSteps;
            set => SetProperty(ref _totalSteps, value);
        }

        public override void Execute()
        {
            var startInput = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var started = startInput.TryGetBoolean(out var b) && b;

            if (started)
            {
                if (CurrentStep < TotalSteps)
                {
                    CurrentStep++;
                    if (Output.Count > 0)
                        Output[0].Value = VariantValue.FromBoolean(true);  // 序列中...
                    if (Output.Count > 1)
                        Output[1].Value = VariantValue.FromBoolean(false); // 未完成
                }
                else
                {
                    // 所有步骤完成，重置
                    CurrentStep = 0;
                    if (Output.Count > 0)
                        Output[0].Value = VariantValue.FromBoolean(false);
                    if (Output.Count > 1)
                        Output[1].Value = VariantValue.FromBoolean(true);  // 完成
                }
            }
        }
    }
}
