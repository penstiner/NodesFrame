using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shell.Models;
using Shell.Services;

namespace Shell.ViewModels
{
    /// <summary>
    /// 工具箱视图模型，提供可拖入编辑器的节点类型列表。
    /// 节点列表由 NodeRegistry 动态生成，同时保留内置硬编码项以保证向后兼容。
    /// </summary>
    public class ToolBoxViewModel
    {
        public ObservableCollection<ToolBoxCategory> Categories { get; } = new();

        // ── 搜索 ──
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                ApplyFilter();
            }
        }

        private List<ToolBoxCategory> _allCategories = new();

        public ToolBoxViewModel()
        {
            // ── 优先从 NodeRegistry 生成分类 ──
            var registryCategories = NodeRegistry.GetAllCategories();
            var registryNodes = NodeRegistry.RegisteredNodes;

            if (registryNodes.Count > 0)
            {
                // 排除算术和运动控制分类
                var excludedCategories = new HashSet<string> { "算术", "运动控制" };
                foreach (var catName in registryCategories.Where(c => !excludedCategories.Contains(c)))
                {
                    var items = registryNodes
                        .Where(n => n.Category == catName)
                        .Select(n => new ToolBoxItem
                        {
                            NodeType = n.NodeTypeId,
                            DisplayName = n.DisplayName,
                            DefaultTitle = n.DefaultTitle,
                            Description = n.Description,
                            Category = catName
                        })
                        .ToList();

                    if (items.Count > 0)
                    {
                        Categories.Add(new ToolBoxCategory
                        {
                            Name = catName,
                            Items = new ObservableCollection<ToolBoxItem>(items)
                        });
                    }
                }
            }

            // ── 向后兼容：追加内置节点分类（如果注册表为空） ──
            if (Categories.Count == 0)
            {
                AddBuiltInCategories();
            }

            // ── 保存全部数据副本用于搜索恢复 ──
            _allCategories = Categories.Select(c => new ToolBoxCategory
            {
                Name = c.Name,
                Items = new ObservableCollection<ToolBoxItem>(c.Items)
            }).ToList();
        }

        private void ApplyFilter()
        {
            Categories.Clear();
            var filter = _searchText.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(filter))
            {
                foreach (var cat in _allCategories)
                    Categories.Add(new ToolBoxCategory { Name = cat.Name, Items = new ObservableCollection<ToolBoxItem>(cat.Items) });
                return;
            }

            foreach (var cat in _allCategories)
            {
                var matching = cat.Items
                    .Where(i => i.DisplayName.ToLowerInvariant().Contains(filter)
                             || i.Description.ToLowerInvariant().Contains(filter)
                             || i.Category.ToLowerInvariant().Contains(filter))
                    .ToList();

                if (matching.Count > 0)
                    Categories.Add(new ToolBoxCategory { Name = cat.Name, Items = new ObservableCollection<ToolBoxItem>(matching) });
            }
        }

        private void AddBuiltInCategories()
        {
            // 兜底：若 [Node] 属性注册为空，手动补回核心流程控制项
            var flowCategory = new ToolBoxCategory { Name = "流程控制" };
            flowCategory.Items.Add(new ToolBoxItem
            {
                NodeType = "Delay",
                DisplayName = "延时",
                DefaultTitle = "延时",
                Description = "将输入值延时传递到输出",
                Category = "流程控制"
            });
            flowCategory.Items.Add(new ToolBoxItem
            {
                NodeType = "Condition",
                DisplayName = "判断",
                DefaultTitle = "判断",
                Description = "比较输入值与阈值，路由到满足/不满足分支",
                Category = "流程控制"
            });
            flowCategory.Items.Add(new ToolBoxItem
            {
                NodeType = "Loop",
                DisplayName = "循环",
                DefaultTitle = "循环",
                Description = "将输入值按循环次数累乘输出",
                Category = "流程控制"
            });
            Categories.Add(flowCategory);
        }
    }
}
