using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nodify;

namespace Shell.Models
{
    /// <summary>
    /// 输出连接器的变量绑定信息。
    /// 用于节点编辑器的"输出绑定"面板，将计算结果写回全局变量。
    /// </summary>
    public sealed class OutputBindingInfo : ObservableObject
    {
        private bool _isBound;
        /// <summary>是否启用输出绑定。</summary>
        public bool IsBound
        {
            get => _isBound;
            set => SetProperty(ref _isBound, value);
        }

        private string _boundVariableName = "";
        /// <summary>绑定的目标变量名。</summary>
        public string BoundVariableName
        {
            get => _boundVariableName;
            set => SetProperty(ref _boundVariableName, value);
        }

        /// <summary>输出连接器的标题（如"输出图像"、"圆数量"）。</summary>
        public string ConnectorTitle { get; }

        /// <summary>输出连接器在 Output 集合中的索引。</summary>
        public int ConnectorIndex { get; }

        /// <summary>可用于绑定的变量名列表（从全局 VariableManager 获取）。</summary>
        public IReadOnlyList<string> AvailableVariables
        {
            get
            {
                var vm = NodeViewModel.GlobalVariableManager;
                if (vm == null) return Array.Empty<string>();
                return vm.GetVariableNames();
            }
        }

        public OutputBindingInfo(string connectorTitle, int connectorIndex)
        {
            ConnectorTitle = connectorTitle;
            ConnectorIndex = connectorIndex;

            // 监听 IsBound 变化，刷新变量列表（新变量出现时需更新下拉框）
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsBound) && _isBound)
                    OnPropertyChanged(nameof(AvailableVariables));
            };
        }

        /// <summary>从 PropertyBinding 恢复绑定状态（反序列化用）。</summary>
        internal void RestoreFrom(PropertyBinding binding)
        {
            if (binding == null) return;
            _isBound = binding.IsBound;
            _boundVariableName = binding.BoundVariableName ?? "";
            OnPropertyChanged(nameof(IsBound));
            OnPropertyChanged(nameof(BoundVariableName));
        }

        /// <summary>导出为 PropertyBinding（序列化用）。</summary>
        internal PropertyBinding ToPropertyBinding()
        {
            return new PropertyBinding
            {
                IsBound = _isBound,
                BoundVariableName = _boundVariableName ?? ""
            };
        }

        /// <summary>刷新可用变量列表（VariableManager 变更时由外部触发）。</summary>
        public void RefreshAvailableVariables()
        {
            OnPropertyChanged(nameof(AvailableVariables));
        }
    }
}
