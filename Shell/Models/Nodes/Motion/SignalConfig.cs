using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    public sealed class SignalConfig : FilteredConfigBase<IOParameter, SignalConfig>
    {
        /// <summary>信号 RegID（JSON 序列化用，映射到基类 Id）。</summary>
        [JsonPropertyName("RegId")]
        public int RegId
        {
            get => Id;
            set => Id = value;
        }

        private int _condition;
        /// <summary>判断条件：0=低电平有效，1=高电平有效。</summary>
        public int Condition
        {
            get => _condition;
            set { _condition = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConditionText)); }
        }

        /// <summary>判定条件显示文本。</summary>
        [JsonIgnore]
        public string ConditionText => Condition == (int)IO_STATUS.OFF ? "OFF" : "ON";

        // ── 子类实现 ──

        protected override IList<IOParameter>? GetAllItems() => CardManager.Card?.InBitList;
        protected override int GetItemId(IOParameter item) => item.RegID;
        protected override string? GetItemName(IOParameter item) => item.Name;
    }
}

