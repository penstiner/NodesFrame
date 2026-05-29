using System.Linq;
using Shell.Models.Attributes;

namespace Shell.Models
{
    [Node(Category = "流程控制", DisplayName = "循环 🔄", DefaultTitle = "循环",
          Description = "将输入值按循环次数累乘输出", NodeTypeId = "Loop")]
    [NodeConnector(Title = "输入", Direction = ConnectorDirection.Input, ExpectedType = "Double")]
    [NodeConnector(Title = "输出", Direction = ConnectorDirection.Output, ExpectedType = "Double")]
    public class LoopNodeViewModel : NodeViewModel
    {
        public LoopNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "输入" });
            AddOutputConnector(new ConnectorViewModel { Title = "输出" });
        }

        private int _loopCount = 3;
        public int LoopCount
        {
            get => _loopCount;
            set => SetProperty(ref _loopCount, value);
        }

        public override void Execute()
        {
            var val = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var dVal = val.TryGetDouble(out var d) ? d : 0;
            if (Output.Count > 0)
                Output[0].Value = dVal * LoopCount;
        }
    }
}
