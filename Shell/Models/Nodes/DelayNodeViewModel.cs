using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shell.Models.Attributes;

namespace Shell.Models
{
    [Node(Category = "流程控制", DisplayName = "延时 ⏱", DefaultTitle = "延时",
          Description = "将输入值延时传递到输出", NodeTypeId = "Delay")]
    [NodeConnector(Title = "输入", Direction = ConnectorDirection.Input, ExpectedType = "Double")]
    [NodeConnector(Title = "输出", Direction = ConnectorDirection.Output, ExpectedType = "Double")]
    public class DelayNodeViewModel : NodeViewModel
    {
        public DelayNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "输入" });
            AddOutputConnector(new ConnectorViewModel { Title = "输出" });
        }

        private int _delayMs = 1000;
        public int DelayMs
        {
            get => _delayMs;
            set => SetProperty(ref _delayMs, value);
        }

        public string DelayDisplay => $"{DelayMs} ms";

        public override void Execute()
        {
            var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            Thread.Sleep(DelayMs);
            if (Output.Count > 0)
                Output[0].Value = inputVal;
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            await Task.Delay(DelayMs, ct);
            if (Output.Count > 0)
                Output[0].Value = inputVal;
        }
    }
}
