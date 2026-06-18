using System;
using System.Linq;
using Hardware.Card.Interface;
using Shell.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 运动控制节点基类。预设 1 个 Boolean 输入 + 1 个 Boolean 输出，
    /// 提供控制卡访问和输入输出便捷方法。多输出节点可在子类构造函数中追加。
    /// </summary>
    public abstract class MotionNodeBase : NodeViewModel
    {
        protected MotionNodeBase()
        {
            AddInputConnector(new ConnectorViewModel { Title = "触发", ExpectedType = TypeCode.Boolean });
            AddOutputConnector(new ConnectorViewModel { Title = "结果", ExpectedType = TypeCode.Boolean });
        }

        protected IControlCard? Card => CardManager.Card;

        protected bool CardReady => Card != null && Card.Initialized;

        /// <summary>读取指定位置的 Boolean 输入值。</summary>
        protected bool GetInputBool(int index = 0)
        {
            var val = Input.ElementAtOrDefault(index)?.Value ?? VariantValue.Null;
            return val.TryGetBoolean(out var b) && b;
        }

        /// <summary>设置指定位置的 Boolean 输出值。</summary>
        protected void SetOutputBool(bool value, int index = 0)
        {
            if (index < Output.Count)
                Output[index].Value = VariantValue.FromBoolean(value);
        }
    }
}
