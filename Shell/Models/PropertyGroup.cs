using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shell.Models
{
    /// <summary>属性分组，用于通用编辑器的分组折叠显示。</summary>
    public sealed class PropertyGroup : INotifyPropertyChanged
    {
        public string Name { get; }
        public ObservableCollection<PropertyItem> Items { get; } = new();

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExpandIcon)); }
        }

        public string ExpandIcon => IsExpanded ? "▼" : "▶";
        public bool HasName => !string.IsNullOrEmpty(Name);

        public PropertyGroup(string name) { Name = name ?? ""; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
