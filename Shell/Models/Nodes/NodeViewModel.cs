using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Nodify;

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
