using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>限位传感器检测配置：选择轴 + 传感器类型 + 期望状态。</summary>
    public sealed class SensorCheckConfig : FilteredConfigBase<AxisParameter, SensorCheckConfig>
    {
        [JsonPropertyName("AxisId")]
        public int AxisId { get => Id; set => Id = value; }

        private int _sensorType;
        /// <summary>0 = 原点(ORG), 1 = 正限位(PEL), 2 = 负限位(NEL)。</summary>
        public int SensorType
        {
            get => _sensorType;
            set { _sensorType = value; OnPropertyChanged(); }
        }

        private int _expectedState;
        /// <summary>0 = ON(低电平), 1 = OFF(高电平)，对应 <see cref="IO_STATUS"/>。</summary>
        public int ExpectedState
        {
            get => _expectedState;
            set { _expectedState = value; OnPropertyChanged(); }
        }

        protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
        protected override int GetItemId(AxisParameter item) => item.RegID;
        protected override string? GetItemName(AxisParameter item) => item.Name;
    }
}
