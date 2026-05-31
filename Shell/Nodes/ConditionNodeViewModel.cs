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
    public class ConditionNodeViewModel : NodeViewModel, IBranchNode
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

        // ── IBranchNode ──
        public int ActiveOutputIndex { get; private set; }

        // ── 判断模式 ──
        [NodeProperty(Key = "mode", DisplayName = "判断模式", Group = "条件设置",
            Options = "数值比较,变量判断")]
        public string Mode { get; set; } = "数值比较";

        // ── 条件变量名（仅 VariableCheck 模式） ──
        [NodeProperty(Key = "conditionVariable", DisplayName = "条件变量", Group = "条件设置")]
        public string ConditionVariableName { get; set; } = "";

        // ── 原有属性保留 ──
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
            bool result;

            if (Mode == "变量判断")
            {
                // 从 GlobalVariableManager 读取布尔变量
                var variable = GlobalVariableManager?.GetVariable(ConditionVariableName);
                result = variable?.Value.TryGetBoolean(out var b) == true && b;
            }
            else
            {
                // 原有数值比较逻辑
                var val = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
                var dVal = val.TryGetDouble(out var d) ? d : 0;

                result = Operation switch
                {
                    CompareOp.GreaterThan => dVal > Threshold,
                    CompareOp.LessThan => dVal < Threshold,
                    CompareOp.Equal => Math.Abs(dVal - Threshold) < 0.0001,
                    CompareOp.NotEqual => Math.Abs(dVal - Threshold) >= 0.0001,
                    CompareOp.GreaterOrEqual => dVal >= Threshold,
                    CompareOp.LessOrEqual => dVal <= Threshold,
                    _ => false
                };
            }

            ActiveOutputIndex = result ? 0 : 1;
            Output[0].Value = result ? VariantValue.FromBoolean(true) : VariantValue.Null;
            Output[1].Value = result ? VariantValue.Null : VariantValue.FromBoolean(true);
        }
    }
}
