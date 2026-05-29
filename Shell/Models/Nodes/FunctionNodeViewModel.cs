using System.Linq;
using Shell.Models.Attributes;

namespace Shell.Models
{
    [Node(Category = "算术", DisplayName = "加法 +", DefaultTitle = "加法",
          Description = "两数相加 (A + B)", NodeTypeId = "Function")]
    [NodeConnector(Title = "A", Direction = ConnectorDirection.Input, ExpectedType = "Double")]
    [NodeConnector(Title = "B", Direction = ConnectorDirection.Input, ExpectedType = "Double")]
    [NodeConnector(Title = "Result", Direction = ConnectorDirection.Output, ExpectedType = "Double")]
    public class FunctionNodeViewModel : NodeViewModel
    {
        private FunctionOp _op = FunctionOp.Add;
        public FunctionOp Op
        {
            get => _op;
            set
            {
                if (SetProperty(ref _op, value))
                {
                    OnPropertyChanged(nameof(OpDisplayName));
                    OnPropertyChanged(nameof(Expression));
                    // 同步更新节点标题为运算名称
                    Title = OpDisplayName;
                }
            }
        }

        public string OpDisplayName => Op switch
        {
            FunctionOp.Add => "加法",
            FunctionOp.Subtract => "减法",
            FunctionOp.Multiply => "乘法",
            FunctionOp.Divide => "除法",
            _ => "?"
        };

        public string Expression => Op switch
        {
            FunctionOp.Add => "f(x) = A + B",
            FunctionOp.Subtract => "f(x) = A - B",
            FunctionOp.Multiply => "f(x) = A × B",
            FunctionOp.Divide => "f(x) = A ÷ B",
            _ => "f(x) = ?"
        };

        public FunctionNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "A" });
            AddInputConnector(new ConnectorViewModel { Title = "B" });
            AddOutputConnector(new ConnectorViewModel { Title = "Result" });
        }

        public override void Execute()
        {
            // 使用 TryGetDouble 安全地从 VariantValue 中提取数值
            var a = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var b = Input.ElementAtOrDefault(1)?.Value ?? VariantValue.Null;

            // 默认未连接时值为 0（向后兼容）
            double av = a.TryGetDouble(out var d1) ? d1 : 0;
            double bv = b.TryGetDouble(out var d2) ? d2 : 0;

            double result = Op switch
            {
                FunctionOp.Add => av + bv,
                FunctionOp.Subtract => av - bv,
                FunctionOp.Multiply => av * bv,
                FunctionOp.Divide => (bv != 0) ? (av / bv) : double.NaN,
                _ => 0
            };

            if (Output.Count > 0)
                Output[0].Value = result;
        }
    }
}
