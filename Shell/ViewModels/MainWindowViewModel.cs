using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Nodify.UndoRedo;
using Prism.Commands;
using Prism.Mvvm;
using Shell.Models;
using Shell.Services;

namespace Shell.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IGraphExecutor _executor;
        private readonly IGraphSerializer _serializer;

        // Expose a collection so NodifyEditor.ItemsSource can enumerate nodes
        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();

        // 连接集合（用于在视图层绑定到 NodifyEditor.Connections）
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

        // 预备连接的视图模型，用于处理拖动创建连接的开始/结束命令
        public PendingConnectionViewModel PendingConnection { get; }

        // 断开连接命令（当连接器请求断开时调用 / ALT+单击）
        public ICommand DisconnectConnectorCommand { get; }

        // 删除指定连接命令（NodifyEditor.RemoveConnectionCommand）
        public ICommand DeleteConnectionCommand { get; }

        // 执行命令：运行图上节点的运算
        public ICommand ExecuteCommand { get; }

        // 自动布局命令
        public ICommand AutoLayoutCommand { get; }

        // 撤销 / 重做命令
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        // 保存 / 加载命令
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }

        // 清空编辑器命令
        public ICommand ClearCommand { get; }

        // 主题切换命令
        public ICommand ToggleThemeCommand { get; }

        // 删除选中项命令（节点和连接）
        public ICommand DeleteSelectedCommand { get; }

        /// <summary>NodifyEditor 选中节点集合（双向绑定）。</summary>
        public ObservableCollection<NodeViewModel> SelectedNodes { get; } = new ObservableCollection<NodeViewModel>();

        /// <summary>安全获取选中节点的预览图像（空集合返回 null，不抛异常）。</summary>
        public System.Windows.Media.Imaging.BitmapImage? SelectedPreviewImage
        {
            get
            {
                if (SelectedNodes.Count == 0) return null;
                var node = SelectedNodes[0];
                var prop = node.GetType().GetProperty("PreviewImage");
                return prop?.GetValue(node) as System.Windows.Media.Imaging.BitmapImage;
            }
        }

        /// <summary>安全获取选中节点的图像信息（空集合返回提示文本）。</summary>
        public string SelectedImageInfo
        {
            get
            {
                if (SelectedNodes.Count == 0) return "点击选择一个图像节点查看预览";
                var node = SelectedNodes[0];
                var prop = node.GetType().GetProperty("ImageInfo");
                if (prop?.GetValue(node) is string s && !string.IsNullOrEmpty(s)) return s;
                return $"节点: {node.Title}";
            }
        }

        /// <summary>工具箱，列出可拖入编辑器的节点类型。</summary>
        public ToolBoxViewModel ToolBox { get; } = new ToolBoxViewModel();

        /// <summary>
        /// 图级别操作历史（添加/删除节点和连接），
        /// 节点属性变更由 Undoable.Global 追踪。
        /// </summary>
        public ActionsHistory GraphHistory { get; } = new ActionsHistory();

        private string _executionError = string.Empty;
        /// <summary>
        /// 执行图计算后的错误消息（空表示无错误）。
        /// </summary>
        public string ExecutionError
        {
            get => _executionError;
            set => SetProperty(ref _executionError, value);
        }

        public MainWindowViewModel(IGraphExecutor executor, IGraphSerializer serializer)
        {
            _executor = executor;
            _serializer = serializer;

            // 选中节点变化时刷新预览属性
            SelectedNodes.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(SelectedPreviewImage));
                RaisePropertyChanged(nameof(SelectedImageInfo));
                RaisePropertyChanged(nameof(IsCompareMode));
                RaisePropertyChanged(nameof(IsSingleMode));
                RaisePropertyChanged(nameof(SecondPreviewImage));
                RaisePropertyChanged(nameof(SecondImageInfo));
            };

            // 绑定命令
            ExecuteCommand = new Nodify.AsyncDelegateCommand(ExecuteAllAsync);
            AutoLayoutCommand = new DelegateCommand(AutoLayoutNodes);
            UndoCommand = new DelegateCommand(() => GraphHistory.Undo(), () => GraphHistory.CanUndo);
            RedoCommand = new DelegateCommand(() => GraphHistory.Redo(), () => GraphHistory.CanRedo);
            SaveCommand = new DelegateCommand(SaveGraph);
            LoadCommand = new DelegateCommand(LoadGraph);
            ClearCommand = new DelegateCommand(ClearAll);
            ToggleThemeCommand = new DelegateCommand(ToggleTheme);

            // 启动时应用默认深色主题
            ApplyTheme("Dark");
            DeleteSelectedCommand = new DelegateCommand(DeleteSelected);
            DeleteConnectionCommand = new DelegateCommand<ConnectionViewModel>(DeleteConnection);

            // 当历史状态变化时刷新撤销/重做按钮可用性
            GraphHistory.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GraphHistory.CanUndo) || e.PropertyName == nameof(GraphHistory.CanRedo))
                {
                    ((DelegateCommand)UndoCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)RedoCommand).RaiseCanExecuteChanged();
                }
            };

            // 创建 PendingConnection 并绑定断开连接命令
            PendingConnection = new PendingConnectionViewModel(this);
            DisconnectConnectorCommand = new DelegateCommand<Shell.Models.ConnectorViewModel>(connector =>
            {
                // 删除该连接器上的所有关联连接
                var toRemove = Connections.Where(x => x.Source == connector || x.Target == connector).ToList();
                foreach (var conn in toRemove)
                {
                    // 记录撤销操作
                    var source = conn.Source;
                    var target = conn.Target;
                    GraphHistory.Record(
                        () => RemoveConnectionInternal(conn),
                        () => RestoreConnection(source, target),
                        "删除连接");
                    RemoveConnectionInternal(conn);
                }
            });
        }

        // helper to add nodes at runtime (with undo support)
        public void AddNode(NodeViewModel node)
        {
            Nodes.Add(node);
            // Record(redo, undo): 添加 → 撤销=删除, 重做=再添加
            GraphHistory.Record(
                () => Nodes.Add(node),
                () => Nodes.Remove(node),
                $"添加节点 {node.Title}");
        }

        /// <summary>
        /// 从工具箱拖放创建节点。根据 ToolBoxItem.NodeType 使用 NodeFactory 创建对应节点。
        /// 优先使用 NodeFactory 注册表，回退到硬编码类型判断。
        /// </summary>
        /// <param name="item">工具箱中被拖放的项。</param>
        /// <param name="graphPosition">编辑器中落点的图坐标。</param>
        public void AddNodeFromToolBox(ToolBoxItem item, Point graphPosition)
        {
            if (item == null) return;

            var factory = new NodeFactory();
            NodeViewModel node = factory.CreateNode(item.NodeType);

            if (node != null)
            {
                node.Location = graphPosition;
                node.Title = item.DefaultTitle;
            }
            else
            {
                // 回退到硬编码类型判断（向后兼容）
                node = item.NodeType switch
                {
                    "Constant" => new ConstantNodeViewModel
                    {
                        Title = item.DefaultTitle,
                        Location = graphPosition,
                        Constant = 0
                    },
                    "Function" => CreateFunctionNode(item.DisplayName, item.DefaultTitle, graphPosition),
                    "Display" => new DisplayNodeViewModel
                    {
                        Title = item.DefaultTitle,
                        Location = graphPosition
                    },
                    "Delay" => new DelayNodeViewModel
                    {
                        Title = item.DefaultTitle,
                        Location = graphPosition
                    },
                    "Condition" => new ConditionNodeViewModel
                    {
                        Title = item.DefaultTitle,
                        Location = graphPosition
                    },
                    "Loop" => new LoopNodeViewModel
                    {
                        Title = item.DefaultTitle,
                        Location = graphPosition
                    },
                    _ => null
                };
            }

            if (node != null)
                AddNode(node);
        }

        /// <summary>
        /// 根据工具箱中的运算名称创建对应 FunctionNodeViewModel。
        /// </summary>
        private FunctionNodeViewModel CreateFunctionNode(string displayName, string defaultTitle, Point location)
        {
            FunctionOp op = displayName switch
            {
                "加法 +" => FunctionOp.Add,
                "减法 −" => FunctionOp.Subtract,
                "乘法 ×" => FunctionOp.Multiply,
                "除法 ÷" => FunctionOp.Divide,
                _ => FunctionOp.Add
            };

            return new FunctionNodeViewModel
            {
                Title = defaultTitle,
                Op = op,
                Location = location
            };
        }

        /// <summary>
        /// 移除节点及其所有关联连接（支持撤销）。
        /// </summary>
        public void RemoveNode(NodeViewModel node)
        {
            if (node == null) return;

            // 收集关联连接以便撤销时恢复
            var relatedConns = Connections.Where(c => c.Source.ParentNode == node || c.Target.ParentNode == node).ToList();

            using (GraphHistory.Batch($"删除节点 {node.Title}"))
            {
                // 先移除关联连接
                foreach (var conn in relatedConns)
                {
                    var src = conn.Source;
                    var tgt = conn.Target;
                    GraphHistory.Record(
                        () => RemoveConnectionInternal(conn),
                        () => RestoreConnection(src, tgt),
                        "删除关联连接");
                    RemoveConnectionInternal(conn);
                }

                // 再移除节点
                var removedNode = node;
                GraphHistory.Record(
                    () => Nodes.Remove(removedNode),
                    () => Nodes.Add(removedNode),
                    $"删除节点 {node.Title}");
                Nodes.Remove(node);
            }
        }

        /// <summary>
        /// 通过 UI 或代码创建一个连接，负责校验有效性、设置连接器的 IsConnected 并将连接加入集合。
        /// </summary>
        /// <returns>连接成功返回 true，否则 false。</returns>
        public bool Connect(Shell.Models.ConnectorViewModel source, Shell.Models.ConnectorViewModel target)
        {
            if (source == null || target == null) return false;

            // 校验 1：不能连接自己
            if (source == target) return false;

            // 校验 2：源必须是输出，目标必须是输入（通过所属节点集合判断）
            bool sourceIsOutput = source.ParentNode?.Output.Contains(source) == true;
            bool targetIsInput = target.ParentNode?.Input.Contains(target) == true;
            if (!sourceIsOutput || !targetIsInput)
            {
                Debug.WriteLine($"[Connect] 连接类型不匹配：source(IsOutput={sourceIsOutput}), target(IsInput={targetIsInput})");
                return false;
            }

            // 校验 3：目标输入不能已有连接（一个输入只能接一条线）
            if (target.IsConnected)
                return false;

            // 校验 4：防止重复连接
            if (Connections.Any(c => c.Source == source && c.Target == target))
                return false;

            // 校验 5：防止形成循环依赖
            if (_executor.WouldCreateCycle(Nodes.ToList(), Connections.ToList(), source, target))
            {
                Debug.WriteLine("[Connect] 连接将形成循环依赖，已阻止。");
                ExecutionError = "无法创建连接：将形成循环依赖。";
                return false;
            }

            // 标记为已连接
            source.IsConnected = true;
            target.IsConnected = true;

            // 添加连接对象，ConnectionViewModel 会在构造时同步值
            var connection = new ConnectionViewModel(source, target);
            Connections.Add(connection);

            // Record(redo, undo): 添加连接 → 撤销=删除, 重做=恢复
            var capturedConn = connection;
            GraphHistory.Record(
                () => RestoreConnection(source, target),
                () => RemoveConnectionInternal(capturedConn),
                "添加连接");

            ExecutionError = null;
            return true;
        }

        /// <summary>
        /// 移除连接（不记录历史，由调用方处理）。
        /// </summary>
        private void RemoveConnectionInternal(ConnectionViewModel conn)
        {
            conn.Source.IsConnected = false;
            conn.Target.IsConnected = false;
            Connections.Remove(conn);
        }

        /// <summary>
        /// 恢复连接（不记录历史，由调用方处理）。
        /// </summary>
        private void RestoreConnection(ConnectorViewModel source, ConnectorViewModel target)
        {
            source.IsConnected = true;
            target.IsConnected = true;
            var conn = new ConnectionViewModel(source, target);
            Connections.Add(conn);
        }

        /// <summary>
        /// 使用 GraphExecutor 拓扑排序执行整张图的计算。
        /// </summary>
        private async Task ExecuteAllAsync()
        {
            ExecutionError = null;

            var snapshotNodes = Nodes.ToList();
            var snapshotConns = Connections.ToList();

            var result = await Task.Run(() =>
                _executor.ExecuteAsync(snapshotNodes, snapshotConns));

            if (!result.Success)
            {
                ExecutionError = result.ErrorMessage;
                Debug.WriteLine($"[Execute] {result.ErrorMessage}");
            }
            else
            {
                Debug.WriteLine($"[Execute] 成功执行 {result.ExecutionOrder.Count} 个节点。");
            }
        }

        // ── 图像对比模式 ──

        public bool IsCompareMode => SelectedNodes.Count == 2
            && SelectedNodes[0].GetType().GetProperty("PreviewImage") != null
            && SelectedNodes[1].GetType().GetProperty("PreviewImage") != null;

        public bool IsSingleMode => !IsCompareMode;

        public System.Windows.Media.Imaging.BitmapImage? SecondPreviewImage
        {
            get
            {
                if (!IsCompareMode) return null;
                var prop = SelectedNodes[1].GetType().GetProperty("PreviewImage");
                return prop?.GetValue(SelectedNodes[1]) as System.Windows.Media.Imaging.BitmapImage;
            }
        }

        public string SecondImageInfo
        {
            get
            {
                if (!IsCompareMode) return "";
                var node = SelectedNodes[1];
                var prop = node.GetType().GetProperty("ImageInfo");
                if (prop?.GetValue(node) is string s && !string.IsNullOrEmpty(s)) return s;
                return $"节点: {node.Title}";
            }
        }

        // ── 自动布局 ──

        private void AutoLayoutNodes()
        {
            if (Nodes.Count == 0) return;
            var list = Nodes.ToList();

            var inDegree = new Dictionary<NodeViewModel, int>();
            var adjacency = new Dictionary<NodeViewModel, List<NodeViewModel>>();
            foreach (var n in list) { inDegree[n] = 0; adjacency[n] = new List<NodeViewModel>(); }
            foreach (var c in Connections)
            {
                var s = c.Source.ParentNode;
                var t = c.Target.ParentNode;
                if (s != null && t != null && s != t) { adjacency[s].Add(t); inDegree[t]++; }
            }

            var queue = new Queue<NodeViewModel>();
            foreach (var n in list) if (inDegree[n] == 0) queue.Enqueue(n);

            var levels = new Dictionary<NodeViewModel, int>();
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                var lv = levels.GetValueOrDefault(cur, 0);
                foreach (var nb in adjacency[cur])
                {
                    levels[nb] = Math.Max(levels.GetValueOrDefault(nb, 0), lv + 1);
                    inDegree[nb]--;
                    if (inDegree[nb] == 0) queue.Enqueue(nb);
                }
            }

            var groups = new Dictionary<int, List<NodeViewModel>>();
            foreach (var n in list)
            {
                var lv = levels.GetValueOrDefault(n, 0);
                if (!groups.ContainsKey(lv)) groups[lv] = new List<NodeViewModel>();
                groups[lv].Add(n);
            }

            const double hGap = 220, vGap = 150, startX = 80, startY = 60;
            foreach (var kv in groups)
            {
                var y = startY + kv.Key * vGap;
                for (int i = 0; i < kv.Value.Count; i++)
                    kv.Value[i].Location = new Point(startX + i * hGap, y);
            }
        }

        private int _themeIndex;
        private static readonly (string Name, string Bg, string Fg, string Contrast,
            string ConnStroke, string ConnFill, string Grid, string ItemFg,
            string DescFg, string HeaderFg, string Sep, string Border,
            string LogBg, string LogText, string LogBorder, string ImgInfo,
            string ToolHover, string ToolSelected)[] Themes =
        {
            ("Dark",  "#1E1E1E", "#D4D4D4", "#2D2D30", "#667788", "#2D2D30",
             "#3A3A3A", "#D4D4D4", "#808080", "#A0A0A0", "#333337", "#FF3E3E42",
             "#12121E", "#AAAAAA", "#3F3F58", "#1E1E2E",
             "#3A3A4A", "#1E5A8A"),
            ("Light", "#F0F0F0", "#222222", "#E0E0E0", "#556677", "#FFFFFF",
             "#B0B8C8", "#222222", "#444444", "#555555", "#C0C0C0", "#C0C0C0",
             "#E8E8F0", "#444444", "#D0D0D8", "#E0E0E8",
             "#D5DCE8", "#B8CCF0"),
            ("Nodify","#2A1B47", "#E0E0E0", "#3D2B5A", "#8899BB", "#3D2B5A",
             "#4C3180", "#E0E0E0", "#909090", "#B0B0B0", "#553388", "#553388",
             "#1A0F30", "#B0A0C8", "#4C3180", "#251545",
             "#3D2B5A", "#5A3D8A"),
        };

        private void ApplyTheme(string name)
        {
            var t = Themes.FirstOrDefault(x => x.Name == name);
            if (t.Name == null) return;
            var app = Application.Current;
            SetBrush(app, "BackgroundBrush", t.Bg);
            SetBrush(app, "ForegroundBrush", t.Fg);
            SetBrush(app, "ContrastBackgroundBrush", t.Contrast);
            SetBrush(app, "ConnectorStroke", t.ConnStroke);
            SetBrush(app, "ConnectorFill", t.ConnFill);
            SetBrush(app, "EditorGridLinesBrush", t.Grid);
            SetBrush(app, "ToolboxItemForeground", t.ItemFg);
            SetBrush(app, "ToolboxItemDescForeground", t.DescFg);
            SetBrush(app, "ToolboxHeaderForeground", t.HeaderFg);
            SetBrush(app, "ToolboxSeparatorBrush", t.Sep);
            SetBrush(app, "EditorToolbarBorder", t.Border);
            // ── 日志 / 图像预览面板 ──
            SetBrush(app, "LogPanelBackgroundBrush", t.LogBg);
            SetBrush(app, "LogPanelTextBrush", t.LogText);
            SetBrush(app, "LogPanelBorderBrush", t.LogBorder);
            SetBrush(app, "ImagePanelInfoBrush", t.ImgInfo);
            SetBrush(app, "ToolboxItemHoverBrush", t.ToolHover);
            SetBrush(app, "ToolboxItemSelectedBrush", t.ToolSelected);
            Nodify.ThemeManager.SetTheme(name);
        }

        private static void SetBrush(Application app, string key, string colorHex)
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            if (app.Resources[key] is SolidColorBrush b && !b.IsFrozen)
            {
                b.Color = color;
            }
            else
            {
                app.Resources[key] = new SolidColorBrush(color);
            }
        }

        private void ToggleTheme()
        {
            _themeIndex = (_themeIndex + 1) % Themes.Length;
            ApplyTheme(Themes[_themeIndex].Name);
        }

        /// <summary>
        /// 清空编辑器中的所有节点和连接（支持撤销）。
        /// </summary>
        private void ClearAll()
        {
            if (Nodes.Count == 0 && Connections.Count == 0) return;

            using (GraphHistory.Batch("清空编辑器"))
            {
                foreach (var conn in Connections.ToList())
                {
                    var src = conn.Source;
                    var tgt = conn.Target;
                    GraphHistory.Record(
                        () => RemoveConnectionInternal(conn),
                        () => RestoreConnection(src, tgt),
                        "删除连接");
                    RemoveConnectionInternal(conn);
                }
                foreach (var node in Nodes.ToList())
                {
                    GraphHistory.Record(
                        () => Nodes.Remove(node),
                        () => Nodes.Add(node),
                        $"删除节点 {node.Title}");
                    Nodes.Remove(node);
                }
            }
            ExecutionError = null;
            Debug.WriteLine("[Clear] 编辑器已清空");
        }

        /// <summary>
        /// 删除所有选中的节点和连接（Del 键触发，支持撤销）。
        /// </summary>
        public void DeleteSelected()
        {
            var selectedConns = Connections.Where(c => c.IsSelected).ToList();
            var selectedNodes = SelectedNodes.ToList();

            if (selectedConns.Count == 0 && selectedNodes.Count == 0)
                return;

            using (GraphHistory.Batch("删除选中项"))
            {
                // 先删除选中连接
                foreach (var conn in selectedConns)
                {
                    var src = conn.Source;
                    var tgt = conn.Target;
                    GraphHistory.Record(
                        () => RemoveConnectionInternal(conn),
                        () => RestoreConnection(src, tgt),
                        "删除连接");
                    RemoveConnectionInternal(conn);
                }

                // 再删除选中节点
                foreach (var node in selectedNodes)
                {
                    RemoveNode(node);
                }
            }

            SelectedNodes.Clear();
            Debug.WriteLine($"[DeleteSelected] 删除了 {selectedConns.Count} 条连接，{selectedNodes.Count} 个节点");
        }

        /// <summary>
        /// 切换连接的选中状态（单选模式：选中一条时自动取消其他）。
        /// </summary>
        public void ToggleConnectionSelection(ConnectionViewModel conn)
        {
            if (conn == null) return;

            bool wasSelected = conn.IsSelected;

            // 先取消所有连接的选中
            foreach (var c in Connections)
                c.IsSelected = false;

            // 如果之前未选中则选中它，否则保持取消（toggle off）
            if (!wasSelected)
                conn.IsSelected = true;
        }

        /// <summary>
        /// 删除指定连接（ALT+单击连接器 / 右键菜单 / Nodify RemoveConnectionCommand）。
        /// </summary>
        private void DeleteConnection(ConnectionViewModel conn)
        {
            if (conn == null) return;

            var src = conn.Source;
            var tgt = conn.Target;
            GraphHistory.Record(
                () => RemoveConnectionInternal(conn),
                () => RestoreConnection(src, tgt),
                "删除连接");
            RemoveConnectionInternal(conn);
        }

        /// <summary>
        /// 保存当前图为 JSON 文件。
        /// </summary>
        private void SaveGraph()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "流程图文件 (*.flow)|*.flow|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".flow",
                FileName = "graph.flow"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = _serializer.Serialize(Nodes.ToList(), Connections.ToList());
                    System.IO.File.WriteAllText(dialog.FileName, json);
                    Debug.WriteLine($"[Save] 图已保存到 {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    ExecutionError = $"保存失败：{ex.Message}";
                    Debug.WriteLine($"[Save] 错误：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 从 JSON 文件加载图。
        /// </summary>
        private void LoadGraph()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "流程图文件 (*.flow)|*.flow|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".flow"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(dialog.FileName);
                    var (loadedNodes, connectionDatas) = _serializer.Deserialize(json);

                    // 清空当前图（先断连后清空，确保干净）
                    foreach (var conn in Connections.ToList())
                        RemoveConnectionInternal(conn);
                    Nodes.Clear();
                    GraphHistory.Clear();

                    // ── 关键：在节点添加到编辑器之前，先标记将被连接的连接器 ──
                    // 这样 Nodify 首次布局时就会计算 Anchor，而非保持 (0,0)
                    foreach (var cd in connectionDatas)
                    {
                        var srcNode = loadedNodes.FirstOrDefault(n => n.Id == cd.SourceNodeId);
                        var tgtNode = loadedNodes.FirstOrDefault(n => n.Id == cd.TargetNodeId);
                        if (srcNode != null && tgtNode != null &&
                            cd.SourceConnectorIndex < srcNode.Output.Count &&
                            cd.TargetConnectorIndex < tgtNode.Input.Count)
                        {
                            srcNode.Output[cd.SourceConnectorIndex].IsConnected = true;
                            tgtNode.Input[cd.TargetConnectorIndex].IsConnected = true;
                        }
                    }

                    // 添加节点（Nodify 布局时，已标记 IsConnected 的连接器会计算 Anchor）
                    foreach (var node in loadedNodes)
                        Nodes.Add(node);

                    // 立即创建连接（ConnectionViewModel 构造器会再次赋 IsConnected=true，无副作用）
                    int connCount = 0;
                    var nodeDict = Nodes.ToDictionary(n => n.Id);
                    foreach (var cd in connectionDatas)
                    {
                        if (nodeDict.TryGetValue(cd.SourceNodeId, out var srcNode) &&
                            nodeDict.TryGetValue(cd.TargetNodeId, out var tgtNode) &&
                            cd.SourceConnectorIndex < srcNode.Output.Count &&
                            cd.TargetConnectorIndex < tgtNode.Input.Count)
                        {
                            var src = srcNode.Output[cd.SourceConnectorIndex];
                            var tgt = tgtNode.Input[cd.TargetConnectorIndex];
                            Connections.Add(new ConnectionViewModel(src, tgt));
                            connCount++;
                        }
                    }

                    Debug.WriteLine($"[Load] 成功加载 {loadedNodes.Count} 个节点，{connCount} 个连接。");
                }
                catch (Exception ex)
                {
                    ExecutionError = $"加载失败：{ex.Message}";
                    Debug.WriteLine($"[Load] 错误：{ex.Message}");
                }
            }
        }
    }
}
