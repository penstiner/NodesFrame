using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Nodify;
using Shell.Services;

namespace Shell.Models
{
    /// <summary>节点执行状态。</summary>
    public enum ExecutionState
    {
        Idle,
        Running,
        Success,
        Error
    }

    public abstract class NodeViewModel : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>全局变量管理器引用，在 MainWindowViewModel 构造时赋值。</summary>
        public static VariableManager GlobalVariableManager { get; set; }

        /// <summary>全局撤销栈引用，供 DialogService 记录属性变更。</summary>
        public static Nodify.UndoRedo.ActionsHistory? GlobalGraphHistory { get; set; }

        /// <summary>分类 → 默认颜色的映射表。</summary>
        public static readonly Dictionary<string, string> CategoryColors = new Dictionary<string, string>
        {
            ["流程控制"] = "#66BB6A",
            ["数学运算"] = "#42A5F5",
            ["视觉算法"] = "#26C6DA",
            ["硬件采集"] = "#FFC107",
            ["显示输出"] = "#FFA726",
            ["逻辑控制"] = "#AB47BC",
            ["运动控制"] = "#EF5350",
            ["杂项"] = "#78909C"
        };

        protected NodeViewModel()
        {
            PropertyChangedDispatcher = action => action();
        }

        // ── 执行状态 ──
        private ExecutionState _state = ExecutionState.Idle;
        public ExecutionState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _nodeCategoryColor;
        public string NodeCategoryColor
        {
            get => _nodeCategoryColor ?? "#78909C";
            set => SetProperty(ref _nodeCategoryColor, value);
        }

        private Point _location;
        public Point Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public ObservableCollection<ConnectorViewModel> Input { get; set; } = new();
        public ObservableCollection<ConnectorViewModel> Output { get; set; } = new();

        // ── 执行耗时 ──
        private TimeSpan? _executionTime;
        /// <summary>最近一次执行耗时；null 表示尚未执行。</summary>
        public TimeSpan? ExecutionTime
        {
            get => _executionTime;
            set
            {
                if (SetProperty(ref _executionTime, value))
                    OnPropertyChanged(nameof(ExecutionTimeDisplay));
            }
        }

        /// <summary>耗时显示文本，可直接绑定到 UI。</summary>
        public string ExecutionTimeDisplay
        {
            get
            {
                if (_executionTime == null) return "—";
                var t = _executionTime.Value;
                if (t.TotalSeconds >= 1)
                    return $"{t.TotalSeconds:F2} s";
                if (t.TotalMilliseconds >= 1)
                    return $"{t.TotalMilliseconds:F1} ms";
                return $"{(t.Ticks / 10.0):F0} μs";
            }
        }

        protected void AddInputConnector(ConnectorViewModel connector)
        {
            connector.ParentNode = this;
            Input.Add(connector);
        }

        protected void AddOutputConnector(ConnectorViewModel connector)
        {
            connector.ParentNode = this;
            Output.Add(connector);
        }

        public abstract void Execute();

        public virtual Task ExecuteAsync(CancellationToken ct = default)
        {
            Execute();
            return Task.CompletedTask;
        }

        // ── 属性绑定字典（Key = NodeProperty的Key或属性名, Value = 绑定信息） ──
        public Dictionary<string, PropertyBinding> PropertyBindings { get; set; } = new Dictionary<string, PropertyBinding>();

        // ── 反射缓存：Key = 属性键名, Value = PropertyInfo ──
        private Dictionary<string, PropertyInfo>? _cachedBindableProps;
        private Dictionary<string, PropertyInfo> GetBindableProps()
        {
            if (_cachedBindableProps != null) return _cachedBindableProps;
            _cachedBindableProps = new();
            foreach (var prop in GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<Attributes.NodePropertyAttribute>();
                if (attr == null) continue;
                _cachedBindableProps[attr.Key ?? prop.Name] = prop;
            }
            return _cachedBindableProps;
        }

        // ── 输出变量绑定 ──
        // 内部字典：Key = 输出索引字符串，Value = 绑定信息（用于序列化）
        private readonly Dictionary<string, PropertyBinding> _outputBindingDict = new();
        /// <summary>输出绑定字典（Key = 输出索引，用于序列化/反序列化）。</summary>
        public Dictionary<string, PropertyBinding> OutputBindingDict => _outputBindingDict;

        private ObservableCollection<OutputBindingInfo>? _outputBindings;
        /// <summary>
        /// 输出连接器的变量绑定列表（用于 UI 绑定面板）。
        /// 每个 Output 连接器对应一个 OutputBindingInfo。
        /// </summary>
        public ObservableCollection<OutputBindingInfo> OutputBindings
        {
            get
            {
                if (_outputBindings == null)
                    BuildOutputBindings();
                return _outputBindings!;
            }
        }

        /// <summary>根据当前 Output 集合重建输出绑定列表（保留已存储的绑定状态）。</summary>
        public void BuildOutputBindings()
        {
            _outputBindings = new ObservableCollection<OutputBindingInfo>();
            for (int i = 0; i < Output.Count; i++)
            {
                var info = new OutputBindingInfo(Output[i].Title, i);
                // 从内部字典恢复已保存的绑定状态
                if (_outputBindingDict.TryGetValue(i.ToString(), out var savedBinding))
                    info.RestoreFrom(savedBinding);
                // 监听变化同步回字典
                info.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(OutputBindingInfo.IsBound)
                        || e.PropertyName == nameof(OutputBindingInfo.BoundVariableName))
                    {
                        _outputBindingDict[info.ConnectorIndex.ToString()] = info.ToPropertyBinding();
                    }
                };
                _outputBindings.Add(info);
            }
        }

        /// <summary>
        /// 执行前解析变量绑定，将绑定的变量值写入对应属性。
        /// </summary>
        public void ResolveBindings(VariableManager variableManager)
        {
            if (variableManager == null || PropertyBindings.Count == 0)
                return;

            var props = GetBindableProps();

            foreach (var kvp in PropertyBindings)
            {
                var binding = kvp.Value;
                if (!binding.IsBound || string.IsNullOrEmpty(binding.BoundVariableName))
                    continue;

                var variable = variableManager.GetVariable(binding.BoundVariableName);
                if (variable == null)
                    continue;

                var propName = kvp.Key;
                if (!props.TryGetValue(propName, out var propInfo) || !propInfo.CanWrite)
                    continue;

                // 若标记了 SkipBindingResolve，不覆盖属性值（属性存的是变量名）
                var nodeAttr = propInfo.GetCustomAttribute<Attributes.NodePropertyAttribute>();
                if (nodeAttr?.SkipBindingResolve == true)
                    continue;

                try
                {
                    var value = variable.Value.GetValueForType(propInfo.PropertyType);
                    if (value != null)
                        propInfo.SetValue(this, value);
                }
                catch { /* 类型不兼容时忽略 */ }
            }
        }

        // ── 通用属性编辑器（反射扫描 [NodeProperty]，按 Group 分组） ──
        private IReadOnlyList<PropertyGroup> _propertyGroups;

        /// <summary>
        /// 通过反射扫描 [NodeProperty] 标记的属性，按 Group 分组返回。
        /// 供通用编辑模板绑定使用，支持分组折叠显示。
        /// </summary>
        public IReadOnlyList<PropertyGroup> PropertyGroups
        {
            get
            {
                if (_propertyGroups == null)
                {
                    var dict = new Dictionary<string, PropertyGroup>();
                    var type = GetType();
                    foreach (var prop in type.GetProperties())
                    {
                        var attr = prop.GetCustomAttributes(typeof(Attributes.NodePropertyAttribute), true)
                            .FirstOrDefault() as Attributes.NodePropertyAttribute;
                        if (attr == null) continue;

                        var item = new PropertyItem(this, prop, attr);
                        var groupName = attr.Group ?? "";
                        if (!dict.TryGetValue(groupName, out var group))
                        {
                            group = new PropertyGroup(groupName);
                            dict[groupName] = group;
                        }
                        group.Items.Add(item);
                    }
                    _propertyGroups = dict.Values.ToList();
                }
                return _propertyGroups;
            }
        }
    }
}
