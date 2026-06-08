using System;
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
    public sealed class SignalConfig : INotifyPropertyChanged
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

        private int _condition;
        /// <summary>判断条件，值对应 <see cref="IO_STATUS"/>：ON=0（低电平有效），OFF=1（高电平有效）。</summary>
        public int Condition
        {
            get => _condition;
            set { _condition = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConditionText)); }
        }

        /// <summary>判定条件显示文本。</summary>
        [JsonIgnore]
        public string ConditionText => Condition == (int)IO_STATUS.OFF ? "高电平" : "低电平";

        /// <summary>兄弟行集合，由 ViewModel 注入，用于计算过滤列表。</summary>
        [JsonIgnore]
        public ObservableCollection<SignalConfig>? Siblings { get; set; }

        // ═══ 下拉数据源：缓存 + 惰性重算，仅在 NotifyFilteredChanged 时失效 ═══

        private List<IOParameter> _filteredSignals = new();
        private bool _filteredValid;

        /// <summary>ComboBox 下拉数据源，自动排除兄弟行已选的信号。</summary>
        [JsonIgnore]
        public List<IOParameter> FilteredSignals
        {
            get
            {
                // 缓存失效 或 之前因卡未就绪导致空列表但现在卡已就绪 → 重算
                if (!_filteredValid || (_filteredSignals.Count == 0 && CardManager.Card != null))
                {
                    _filteredSignals = BuildFilteredList();
                    _filteredValid = true;
                }
                return _filteredSignals;
            }
        }

        /// <summary>构建过滤列表，对 InBitList 做一次安全快照。</summary>
        private List<IOParameter> BuildFilteredList()
        {
            var all = CardManager.Card?.InBitList;
            if (all == null) return new List<IOParameter>();

            // 唯一需要 try-catch 的地方：硬件线程可能正修改 InBitList
            List<IOParameter> snapshot;
            try { snapshot = new List<IOParameter>(all); }
            catch { return _filteredSignals; }   // 保留旧缓存

            var used = Siblings?.Where(c => c != this).Select(c => c.RegId).ToHashSet() ?? new HashSet<int>();
            return snapshot.Where(s => !used.Contains(s.RegID)).ToList();
        }

        /// <summary>ComboBox 选择绑定用，不序列化。</summary>
        [JsonIgnore]
        public IOParameter? SelectedSignal
        {
            get => CardManager.Card?.InBitList?.FirstOrDefault(s => s.RegID == RegId);
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
