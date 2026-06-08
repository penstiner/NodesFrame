using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 输出信号配置项：绑定一条硬件输出信号 + 目标电平。
    /// </summary>
    public sealed class OutputSignalConfig : INotifyPropertyChanged
    {
        private int _regId;
        public int RegId
        {
            get => _regId;
            set { _regId = value; OnPropertyChanged(); }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private int _targetState;
        /// <summary>
        /// 目标状态：0 = OFF（低电平），1 = ON（高电平）。
        /// </summary>
        public int TargetState
        {
            get => _targetState;
            set { _targetState = value; OnPropertyChanged(); OnPropertyChanged(nameof(TargetStateText)); }
        }

        /// <summary>目标状态显示文本。</summary>
        [JsonIgnore]
        public string TargetStateText => _targetState == 1 ? "ON" : "OFF";

        /// <summary>兄弟行集合，由 ViewModel 注入，用于计算过滤列表。</summary>
        [JsonIgnore]
        public ObservableCollection<OutputSignalConfig>? Siblings { get; set; }

        // ═══ 下拉数据源：缓存 + 惰性重算 ═══

        private List<IOParameter> _filteredSignals = new();
        private bool _filteredValid;

        /// <summary>ComboBox 下拉数据源，自动排除兄弟行已选的信号。</summary>
        [JsonIgnore]
        public List<IOParameter> FilteredSignals
        {
            get
            {
                if (!_filteredValid || (_filteredSignals.Count == 0 && CardManager.Card != null))
                {
                    _filteredSignals = BuildFilteredList();
                    _filteredValid = true;
                }
                return _filteredSignals;
            }
        }

        private List<IOParameter> BuildFilteredList()
        {
            var all = CardManager.Card?.OutBitList;
            if (all == null) return new List<IOParameter>();

            List<IOParameter> snapshot;
            try { snapshot = new List<IOParameter>(all); }
            catch { return _filteredSignals; }

            var used = Siblings?.Where(c => c != this).Select(c => c.RegId).ToHashSet() ?? new HashSet<int>();
            return snapshot.Where(s => !used.Contains(s.RegID)).ToList();
        }

        /// <summary>ComboBox 选择绑定用。</summary>
        [JsonIgnore]
        public IOParameter? SelectedSignal
        {
            get => CardManager.Card?.OutBitList?.FirstOrDefault(s => s.RegID == RegId);
            set
            {
                if (value != null)
                {
                    _regId = value.RegID;
                    _name = value.Name ?? "";
                    OnPropertyChanged(nameof(RegId));
                    OnPropertyChanged(nameof(Name));
                }
                OnPropertyChanged(nameof(SelectedSignal));
            }
        }

        /// <summary>刷新 FilteredSignals（当兄弟行变化时由 ViewModel 调用）。</summary>
        public void NotifyFilteredChanged()
        {
            _filteredValid = false;
            OnPropertyChanged(nameof(FilteredSignals));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
