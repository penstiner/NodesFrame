using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>单轴复位配置项：下拉选轴 + 复位速度。</summary>
    public sealed class ResetAxisConfig : FilteredConfigBase<AxisParameter, ResetAxisConfig>
    {
        [JsonPropertyName("AxisId")]
        public int AxisId { get => Id; set => Id = value; }

        private double _speed = 50;
        public double Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(); }
        }

        protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
        protected override int GetItemId(AxisParameter item) => item.RegID;
        protected override string? GetItemName(AxisParameter item) => item.Name;
    }
}
