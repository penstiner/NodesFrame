using System.Collections.ObjectModel;

namespace Shell.Models
{
    public class ToolBoxCategory
    {
        public string Name { get; set; } = string.Empty;
        public bool IsExpanded { get; set; } = true;
        public ObservableCollection<ToolBoxItem> Items { get; set; } = new();
    }
}
