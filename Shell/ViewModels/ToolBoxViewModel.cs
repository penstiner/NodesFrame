using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Nodify;
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

        /// <summary>清除搜索文本的命令。</summary>
        public ICommand ClearSearchCommand { get; }

        private List<ToolBoxCategory> _allCategories = new();

        /// <summary>保存搜索前各分类的 IsExpanded 状态，搜索清空时恢复。</summary>
        private Dictionary<string, bool> _preSearchExpandedStates = new();

        public ToolBoxViewModel()
        {
            ClearSearchCommand = new DelegateCommand(() => SearchText = string.Empty);

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
                            Category = catName,
                            IconCode = GetIconCodeForNodeType(n.NodeTypeId),
                            IconFontFamily = GetIconFontFamilyForNodeType(n.NodeTypeId),
                            ColorTag = GetColorTagForNodeType(n.NodeTypeId)
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

            // 排序：流程控制 > 输入输出 > 硬件采集 > 视觉算法 > 其他
            var sortedCategories = Categories.OrderBy(c =>
            {
                if (c.Name == "流程控制") return 0;
                if (c.Name == "输入输出") return 1;
                if (c.Name == "硬件采集") return 2;
                if (c.Name == "视觉算法") return 3;
                return 4;
            }).ToList();

            Categories.Clear();
            foreach (var cat in sortedCategories)
                Categories.Add(cat);

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
                // 恢复所有分类，并恢复搜索前的展开状态
                foreach (var cat in _allCategories)
                {
                    var restoredCat = new ToolBoxCategory
                    {
                        Name = cat.Name,
                        Items = new ObservableCollection<ToolBoxItem>(cat.Items)
                    };
                    if (_preSearchExpandedStates.TryGetValue(cat.Name, out var wasExpanded))
                        restoredCat.IsExpanded = wasExpanded;
                    Categories.Add(restoredCat);
                }
                _preSearchExpandedStates.Clear();
                return;
            }

            // 首次搜索时，保存当前各分类的展开状态
            if (_preSearchExpandedStates.Count == 0)
            {
                foreach (var cat in _allCategories)
                    _preSearchExpandedStates[cat.Name] = true; // 默认已展开
            }

            foreach (var cat in _allCategories)
            {
                var matching = cat.Items
                    .Where(i => i.DisplayName.ToLowerInvariant().Contains(filter)
                             || i.Description.ToLowerInvariant().Contains(filter)
                             || i.Category.ToLowerInvariant().Contains(filter))
                    .ToList();

                if (matching.Count > 0)
                {
                    Categories.Add(new ToolBoxCategory
                    {
                        Name = cat.Name,
                        IsExpanded = true,  // 有匹配项的分类自动展开
                        Items = new ObservableCollection<ToolBoxItem>(matching)
                    });
                }
                // 无匹配项的分类不添加，相当于折叠/隐藏
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
                Category = "流程控制",
                IconCode = "\uf017",
                ColorTag = "#FF26A69A"
            });
            flowCategory.Items.Add(new ToolBoxItem
            {
                NodeType = "Condition",
                DisplayName = "判断",
                DefaultTitle = "判断",
                Description = "比较输入值与阈值，路由到满足/不满足分支",
                Category = "流程控制",
                IconCode = "\uf074",
                ColorTag = "#FFAB47BC"
            });
            flowCategory.Items.Add(new ToolBoxItem
            {
                NodeType = "Loop",
                DisplayName = "循环",
                DefaultTitle = "循环",
                Description = "将输入值按循环次数累乘输出",
                Category = "流程控制",
                IconCode = "\uf01e",
                ColorTag = "#FFEF5350"
            });
            Categories.Add(flowCategory);
        }

        /// <summary>
        /// 根据节点类型标识返回对应的图标字体族资源Key。
        /// </summary>
        public static string GetIconFontFamilyForNodeType(string nodeType)
        {
            if (nodeType.StartsWith("Vision."))
                return "iconfont";
            return "FontAwesome";
        }

        /// <summary>
        /// 根据节点类型标识返回对应的 FontAwesome / iconfont 图标字符码。
        /// </summary>
        private static string GetIconCodeForNodeType(string nodeTypeId)
        {
            return nodeTypeId switch
            {
                "Delay" => "\uf017",         // 时钟
                "Condition" => "\uf074",     // 分叉
                "Loop" => "\uf01e",          // 循环
                "Constant" => "\uf1ec",      // 计算器
                "Function" => "\uf121",      // 代码
                "Display" => "\uf06e",       // 眼睛
                "ImageDisplay" => "\uf03e",  // 图片

                // 硬件采集节点（FontAwesome 字体）
                "Hardware.CameraInit" => "\uf030",      // FontAwesome camera
                "Hardware.CameraCapture" => "\uf030",   // FontAwesome camera
                "Hardware.CameraClose" => "\uf057",     // FontAwesome times-circle

                // 流程控制节点（FontAwesome 字体）
                "Flow.Start" => "\uf04b",              // FontAwesome play
                "Flow.End" => "\uf04d",                // FontAwesome stop

                // 视觉算法节点（iconfont 字体）
                "Vision.GaussianBlur" => "\ue634",
                "Vision.MedianBlur" => "\ue73f",
                "Vision.CannyEdge" => "\ue707",
                "Vision.Threshold" => "\ue765",
                "Vision.AdaptiveThreshold" => "\ue656",
                "Vision.BrightnessContrast" => "\ue608",
                "Vision.CvtColor" => "\ue609",
                "Vision.EqualizeHist" => "\ue8b5",
                "Vision.Flip" => "\ue687",
                "Vision.Morphology" => "\ue7b6",
                "Vision.HoughLines" => "\ue7b1",
                "Vision.Resize" => "\ue60f",
                "Vision.ImageSource" => "\ue60a",
                "Vision.ImageDisplay" => "\ue665",

                _ => "\ue634"                // 圆点（默认）
            };
        }

        /// <summary>
        /// 根据节点类型标识返回对应的颜色标签。
        /// </summary>
        private static string GetColorTagForNodeType(string nodeTypeId)
        {
            return nodeTypeId switch
            {
                "Delay" => "#FF26A69A",      // 青绿色
                "Condition" => "#FFAB47BC",  // 紫色
                "Loop" => "#FFEF5350",       // 红色
                "Constant" => "#FF66BB6A",   // 绿色
                "Function" => "#FF42A5F5",   // 蓝色
                "Display" => "#FFFFA726",    // 橙色
                "ImageDisplay" => "#FF26C6DA", // 青色

                // 视觉算法节点颜色
                // 滤波类
                "Vision.GaussianBlur" => "#FF5C6BC0",
                "Vision.MedianBlur" => "#FF5C6BC0",
                // 检测类
                "Vision.CannyEdge" => "#FF26A69A",
                "Vision.HoughLines" => "#FF26A69A",
                // 二值化类
                "Vision.Threshold" => "#FF7E57C2",
                "Vision.AdaptiveThreshold" => "#FF7E57C2",
                // 调整类
                "Vision.BrightnessContrast" => "#FFFFA726",
                "Vision.EqualizeHist" => "#FFFFA726",
                "Vision.CvtColor" => "#FFFFA726",
                // 变换类
                "Vision.Flip" => "#FF42A5F5",
                "Vision.Resize" => "#FF42A5F5",
                "Vision.Morphology" => "#FF42A5F5",
                // 输入输出
                "Vision.ImageSource" => "#FF26C6DA",
                "Vision.ImageDisplay" => "#FF26C6DA",

                // 硬件采集节点 - 靖蓝色
                _ when nodeTypeId.StartsWith("Hardware.") => "#FF5C6BC0",
                
                // 流程控制节点 - 绿色
                _ when nodeTypeId.StartsWith("Flow.") => "#FF66BB6A",

                _ => "#FF78909C"             // 灰色（默认）
            };
        }
    }
}
