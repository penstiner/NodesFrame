using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>单轴运动配置项：下拉选轴 + 定位方式 + 速度 + 目标位置。</summary>
    public sealed class MotorMoveConfig : FilteredConfigBase<AxisParameter, MotorMoveConfig>
    {
        [JsonPropertyName("AxisId")]
        public int AxisId { get => Id; set => Id = value; }

        private int _moveType;
        /// <summary>0=绝对，1=相对。</summary>
        public int MoveType
        {
            get => _moveType;
            set { _moveType = value; OnPropertyChanged(); OnPropertyChanged(nameof(MoveTypeText)); }
        }

        [JsonIgnore]
        public string MoveTypeText => _moveType == 1 ? "相对" : "绝对";

        private double _speed = 50;
        public double Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(); }
        }

        private double _position = 100;
        public double Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
        protected override int GetItemId(AxisParameter item) => item.RegID;
        protected override string? GetItemName(AxisParameter item) => item.Name;
    }
}
