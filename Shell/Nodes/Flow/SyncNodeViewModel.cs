using System;
using System.Threading;
using System.Threading.Tasks;
using Nodify;
using Shell.Models.Attributes;

namespace Shell.Models.Nodes.Flow
{
    /// <summary>
    /// 同步节点：等待所有输入分支的值都为 true 时，输出 true。
    /// 输入端口数量通过 InputCount 属性动态配置（1~16 个）。
    /// </summary>
    [Node(
        Category = "流程控制",
        DisplayName = "同步 ✅",
        DefaultTitle = "同步",
        Description = "当所有输入端口的值都为 true 时，输出 true",
        NodeTypeId = "Flow.Sync")]
    public class SyncNodeViewModel : NodeViewModel
    {
        private int _inputCount = 2;

        public SyncNodeViewModel()
        {
            RebuildInputs(_inputCount);
            AddOutputConnector(new ConnectorViewModel { Title = "输出", ExpectedType = TypeCode.Boolean });
        }

        [NodeProperty(Key = "inputCount", DisplayName = "输入端口数", Group = "参数",
            Min = 1, Max = 16)]
        public int InputCount
        {
            get => _inputCount;
            set
            {
                var clamped = Math.Clamp(value, 1, 16);
                if (clamped == _inputCount) return;
                _inputCount = clamped;
                RebuildInputs(_inputCount);
                OnPropertyChanged(nameof(InputCount));
            }
        }

        /// <summary>根据目标数量重建输入端口。多退少补，多余的 Nodify 会自动断开连接。</summary>
        private void RebuildInputs(int target)
        {
            // 删多余的（从尾部开始）
            while (Input.Count > target)
                Input.RemoveAt(Input.Count - 1);

            // 补不足的
            while (Input.Count < target)
            {
                var idx = Input.Count + 1;
                AddInputConnector(new ConnectorViewModel
                {
                    Title = $"输入 {idx}",
                    ExpectedType = TypeCode.Boolean
                });
            }
        }

        /// <summary>检查所有输入端口：任一为 false 则输出 false。</summary>
        public override void Execute()
        {
            bool allTrue = true;
            foreach (var input in Input)
            {
                if (!input.IsConnected) continue; // 未连接则跳过
                var val = input.Value;
                if (!val.TryGetBoolean(out var b) || !b)
                {
                    allTrue = false;
                    break;
                }
            }

            if (Output.Count > 0)
                Output[0].Value = VariantValue.FromBoolean(allTrue);
        }

        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            Execute();
            await Task.CompletedTask;
        }
    }
}
