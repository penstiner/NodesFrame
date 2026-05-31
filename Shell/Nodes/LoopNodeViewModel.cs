using System.Linq;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models
{
    [Node(Category = "流程控制", DisplayName = "重复N次", DefaultTitle = "重复N次",
          Description = "循环执行体内流程指定次数", NodeTypeId = "Loop")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "接收上游触发信号")]
    [NodeConnector(Title = "循环体", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "每次迭代触发")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "循环完成后触发")]
    public class LoopNodeViewModel : NodeViewModel, ILoopNode
    {
        public LoopNodeViewModel()
        {
            Title = "重复N次";

            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });

            // Output[0] = 循环体
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "循环体",
                ExpectedType = System.TypeCode.Boolean
            });

            // Output[1] = 完成
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "完成",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        private int _loopCount = 3;
        [NodeProperty(Key = "loopCount", DisplayName = "循环次数", Group = "循环设置")]
        public int LoopCount
        {
            get => _loopCount;
            set => SetProperty(ref _loopCount, value);
        }

        /// <summary>当前迭代计数（每次 Execute 递增）。</summary>
        private int _currentIteration;
        public int CurrentIteration
        {
            get => _currentIteration;
            private set => SetProperty(ref _currentIteration, value);
        }

        // IBranchNode / ILoopNode 实现
        public int ActiveOutputIndex { get; private set; }
        public bool IsLooping => ActiveOutputIndex == 0;
        public void OnLoopEnter() { /* 迭代计数已在 Execute 中自增 */ }
        public void OnLoopExit() { Reset(); }
        public string LoopDescription => $"{Title} 第 {CurrentIteration}/{LoopCount} 次迭代";

        /// <summary>重置迭代计数器（FlowExecutor 开始执行前调用）。</summary>
        public void Reset()
        {
            CurrentIteration = 0;
        }

        public override void Execute()
        {
            if (CurrentIteration < LoopCount)
            {
                // 还有剩余次数 → 走循环体
                ActiveOutputIndex = 0;
                Output[0].Value = VariantValue.FromBoolean(true);
                Output[1].Value = VariantValue.Null;
                CurrentIteration++;
                ExecutionLogger.Info("重复N次", $"第 {CurrentIteration}/{LoopCount} 次迭代");
            }
            else
            {
                // 循环完成 → 走完成输出
                ActiveOutputIndex = 1;
                Output[0].Value = VariantValue.Null;
                Output[1].Value = VariantValue.FromBoolean(true);
                ExecutionLogger.Info("重复N次", $"循环完成（共 {LoopCount} 次）");
            }
        }
    }
}
