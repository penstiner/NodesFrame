using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>连续运动配置项：选择轴 + 速度，调用 VMove 启动匀速连续运动。</summary>
    public sealed class VMoveConfig : FilteredConfigBase<AxisParameter, VMoveConfig>
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
