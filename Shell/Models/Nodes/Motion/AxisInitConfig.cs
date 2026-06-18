using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>控制卡初始化后的轴参数配置：脉冲模式 + 传感器逻辑 + 伺服使能。</summary>
    public sealed class AxisInitConfig : FilteredConfigBase<AxisParameter, AxisInitConfig>
    {
        [JsonPropertyName("AxisId")]
        public int AxisId { get => Id; set => Id = value; }

        private int _pulseMode;
        public int PulseMode
        {
            get => _pulseMode;
            set { _pulseMode = value; OnPropertyChanged(); }
        }

        private int _orgLogic;
        /// <summary>原点传感器：0=低有效，1=高有效。</summary>
        public int OrgLogic
        {
            get => _orgLogic;
            set { _orgLogic = value; OnPropertyChanged(); }
        }

        private int _pelLogic;
        /// <summary>正限位传感器：0=低有效，1=高有效。</summary>
        public int PelLogic
        {
            get => _pelLogic;
            set { _pelLogic = value; OnPropertyChanged(); }
        }

        private int _nelLogic;
        /// <summary>负限位传感器：0=低有效，1=高有效。</summary>
        public int NelLogic
        {
            get => _nelLogic;
            set { _nelLogic = value; OnPropertyChanged(); }
        }

        private bool _enableServo = true;
        public bool EnableServo
        {
            get => _enableServo;
            set { _enableServo = value; OnPropertyChanged(); }
        }

        protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
        protected override int GetItemId(AxisParameter item) => item.RegID;
        protected override string? GetItemName(AxisParameter item) => item.Name;
    }
}
