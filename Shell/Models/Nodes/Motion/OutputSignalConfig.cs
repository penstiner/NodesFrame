using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>输出信号配置项：绑定一条硬件输出信号 + 目标电平。</summary>
    public sealed class OutputSignalConfig : FilteredConfigBase<IOParameter, OutputSignalConfig>
    {
        [JsonPropertyName("RegId")]
        public int RegId { get => Id; set => Id = value; }

        private int _targetState;
        /// <summary>目标状态：0=OFF，1=ON。</summary>
        public int TargetState
        {
            get => _targetState;
            set { _targetState = value; OnPropertyChanged(); OnPropertyChanged(nameof(TargetStateText)); }
        }

        [JsonIgnore]
        public string TargetStateText => _targetState == 1 ? "ON" : "OFF";

        protected override IList<IOParameter>? GetAllItems() => CardManager.Card?.OutBitList;
        protected override int GetItemId(IOParameter item) => item.RegID;
        protected override string? GetItemName(IOParameter item) => item.Name;
    }
}
