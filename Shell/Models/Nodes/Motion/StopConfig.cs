using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>急停配置项：选择要停止的轴。</summary>
    public sealed class StopConfig : FilteredConfigBase<AxisParameter, StopConfig>
    {
        [JsonPropertyName("AxisId")]
        public int AxisId { get => Id; set => Id = value; }

        protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
        protected override int GetItemId(AxisParameter item) => item.RegID;
        protected override string? GetItemName(AxisParameter item) => item.Name;
    }
}
