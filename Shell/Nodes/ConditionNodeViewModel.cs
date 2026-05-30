using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Models.Attributes;

namespace Shell.Models
{
    [Node(Category = "流程控制", DisplayName = "判断 ◈", DefaultTitle = "判断",
          Description = "比较输入值与阈值，路由到满足/不满足分支", NodeTypeId = "Condition")]
    [NodeConnector(Title = "输入值", Direction = ConnectorDirection.Input, ExpectedType = "Double")]
    [NodeConnector(Title = "满足", Direction = ConnectorDirection.Output, ExpectedType = "Double")]
    [NodeConnector(Title = "不满足", Direction = ConnectorDirection.Output, ExpectedType = "Double")]
    public class ConditionNodeViewModel : NodeViewModel
    {
        public record CompareOpItem(CompareOp Value, string Symbol);

        public static IReadOnlyList<CompareOpItem> AvailableCompareOps { get; } = new[]
        {
            new CompareOpItem(CompareOp.GreaterThan, "> 大于"),
            new CompareOpItem(CompareOp.LessThan, "< 小于"),
            new CompareOpItem(CompareOp.Equal, "= 等于"),
            new CompareOpItem(CompareOp.NotEqual, "≠ 不等于"),
            new CompareOpItem(CompareOp.GreaterOrEqual, "≥ 大于等于"),
            new CompareOpItem(CompareOp.LessOrEqual, "≤ 小于等于"),
        };

        public ConditionNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "输入值" });
            AddOutputConnector(new ConnectorViewModel { Title = "满足" });
            AddOutputConnector(new ConnectorViewModel { Title = "不满足" });
        }

        private CompareOp _operation = CompareOp.GreaterThan;
        public CompareOp Operation
        {
            get => _operation;
            set
            {
                if (SetProperty(ref _operation, value))
                    OnPropertyChanged(nameof(CompareSymbol));
            }
        }

        private double _threshold;
        public double Threshold
        {
            get => _threshold;
            set => SetProperty(ref _threshold, value);
        }

        public string CompareSymbol => Operation switch
        {
            CompareOp.GreaterThan => ">",
            CompareOp.LessThan => "<",
            CompareOp.Equal => "==",
            CompareOp.NotEqual => "!=",
            CompareOp.GreaterOrEqual => "≥",
            CompareOp.LessOrEqual => "≤",
            _ => "?"
        };

        public override void Execute()
        {
            var val = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var dVal = val.TryGetDouble(out var d) ? d : 0;

            bool result = Operation switch
            {
                CompareOp.GreaterThan => dVal > Threshold,
                CompareOp.LessThan => dVal < Threshold,
                CompareOp.Equal => Math.Abs(dVal - Threshold) < 0.0001,
                CompareOp.NotEqual => Math.Abs(dVal - Threshold) >= 0.0001,
                CompareOp.GreaterOrEqual => dVal >= Threshold,
                CompareOp.LessOrEqual => dVal <= Threshold,
                _ => false
            };
            Output[0].Value = result ? val : VariantValue.Null;
            Output[1].Value = result ? VariantValue.Null : val;
        }
    }
}
