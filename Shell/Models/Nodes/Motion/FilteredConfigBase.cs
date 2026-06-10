using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>
    /// 可过滤配置项基类：提供下拉数据源的缓存、过滤（排除兄弟行已选项）、选择绑定。
    /// 子类只需实现 <see cref="GetAllItems"/> / <see cref="GetItemId"/> / <see cref="GetItemName"/>。
    /// </summary>
    /// <typeparam name="TItem">列表项类型（如 IOParameter, AxisParameter）</typeparam>
    /// <typeparam name="TSelf">自身类型（CRTP 模式，用于 Siblings 类型安全）</typeparam>
    public abstract class FilteredConfigBase<TItem, TSelf> : INotifyPropertyChanged
        where TItem : class
        where TSelf : FilteredConfigBase<TItem, TSelf>
    {
        private int _id = -1;
        private string _name = "";
        private List<TItem> _filteredCache = new();
        private bool _filteredValid;
        private int _lastCardVersion = -1;  // 检测 CardManager 配置变更

        /// <summary>配置项 ID（对应硬件的 RegID），子类通过具体属性暴露给 JSON。</summary>
        [JsonIgnore]
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        /// <summary>配置项名称。</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        /// <summary>兄弟行集合，由 ViewModel 注入，用于过滤已选列表。</summary>
        [JsonIgnore]
        public ObservableCollection<TSelf>? Siblings { get; set; }

        /// <summary>过滤后的下拉数据源（缓存 + 版本检测：硬件配置变更时自动重建）。</summary>
        [JsonIgnore]
        public List<TItem> FilteredItems
        {
            get
            {
                bool configChanged = _lastCardVersion != CardManager.CardVersion;
                if (!_filteredValid || configChanged)
                {
                    _filteredCache = BuildFiltered();
                    _filteredValid = _filteredCache.Count > 0;
                    _lastCardVersion = CardManager.CardVersion;
                }
                return _filteredCache;
            }
        }

        /// <summary>ComboBox 选择绑定。</summary>
        [JsonIgnore]
        public TItem? SelectedItem
        {
            get => GetAllItems()?.FirstOrDefault(item => GetItemId(item) == _id);
            set
            {
                if (value != null)
                {
                    _id = GetItemId(value);
                    _name = GetItemName(value) ?? "";
                    OnPropertyChanged(nameof(Id));
                    OnPropertyChanged(nameof(Name));
                }
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        /// <summary>使过滤缓存失效，下次访问 FilteredItems 时重算。</summary>
        public void NotifyFilteredChanged()
        {
            _filteredValid = false;
            OnPropertyChanged(nameof(FilteredItems));
        }

        private List<TItem> BuildFiltered()
        {
            var all = GetAllItems();
            if (all == null) return new List<TItem>();

            // 安全快照：避免硬件线程修改集合时抛异常
            List<TItem> snapshot;
            try { snapshot = new List<TItem>(all); }
            catch { return _filteredCache; }

            var used = Siblings?.Where(c => !ReferenceEquals(c, this) && c.Id >= 0).Select(c => c.Id).ToHashSet()
                       ?? new HashSet<int>();
            return snapshot.Where(item => !used.Contains(GetItemId(item))).ToList();
        }

        // ── 子类实现 ──

        /// <summary>获取硬件原始列表。</summary>
        protected abstract IList<TItem>? GetAllItems();

        /// <summary>获取数据项的 ID。</summary>
        protected abstract int GetItemId(TItem item);

        /// <summary>获取数据项的名称。</summary>
        protected abstract string? GetItemName(TItem item);

        // ── INotifyPropertyChanged ──

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
