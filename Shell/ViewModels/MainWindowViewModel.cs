using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using HandyControl.Controls;
using HandyControl.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Nodify.UndoRedo;
using Prism.Commands;
using Prism.Mvvm;
using Shell.Models;
using Shell.Models.Nodes.Flow;
using Shell.Services;
using Shell.Views;

namespace Shell.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IGraphSerializer _serializer;
        private FlowExecutor _flowExecutor = new();
        private CancellationTokenSource? _cts;

        /// <summary>变量管理器，供节点绑定变量时使用。</summary>
        public VariableManager VariableManager { get; } = new VariableManager();

        // 节点集合
        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();

        // 连接集合
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

        // 预备连接的视图模型，用于处理拖动创建连接的开始/结束命令
        public PendingConnectionViewModel PendingConnection { get; }

        // 断开连接命令（当连接器请求断开时调用 / ALT+单击）
        public ICommand DisconnectConnectorCommand { get; }

        // 删除指定连接命令（NodifyEditor.RemoveConnectionCommand）
        public ICommand DeleteConnectionCommand { get; }

        // 执行命令：运行图上节点的运算
        public ICommand ExecuteCommand { get; }

        // 停止命令：取消正在执行的流程
        public ICommand StopCommand { get; }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        // 自动布局命令
        public ICommand AutoLayoutCommand { get; }

        // 撤销 / 重做命令
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        // 保存 / 加载命令
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveAsCommand { get; }

        /// <summary>当前已打开/保存的文件路径，用于"保存"直接覆写。</summary>
        private string? _currentFilePath;

        // 清空编辑器命令
        public ICommand ClearCommand { get; }

        // 删除选中项命令（节点和连接）
        public ICommand DeleteSelectedCommand { get; }

        // 变量管理命令
        public ICommand AddVariableCommand { get; }
        public ICommand RemoveVariableCommand { get; }

        // 硬件配置命令
        public ICommand HardwareConfigCommand { get; }

        private Variable _selectedVariable;
        /// <summary>当前在变量管理器中选中的变量。</summary>
        public Variable SelectedVariable
        {
            get => _selectedVariable;
            set => SetProperty(ref _selectedVariable, value);
        }

        /// <summary>NodifyEditor 选中节点集合（双向绑定）。</summary>
        public ObservableCollection<NodeViewModel> SelectedNodes { get; } = new ObservableCollection<NodeViewModel>();

        /// <summary>安全获取选中节点的预览图像（空集合返回 null，不抛异常）。</summary>
        public System.Windows.Media.ImageSource? SelectedPreviewImage
        {
            get
            {
                if (SelectedNodes.Count == 0) return null;
                var node = SelectedNodes[0];
                return GetPreviewImageSource(node);
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

        public MainWindowViewModel(IGraphSerializer serializer)
        {
            _serializer = serializer;

            _flowExecutor.VariableManager = VariableManager;

            // 将 VariableManager 传递给 NodeViewModel，以便属性编辑器获取变量列表
            NodeViewModel.GlobalVariableManager = VariableManager;
            NodeViewModel.GlobalGraphHistory = GraphHistory;

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
            StopCommand = new DelegateCommand(StopFlow);
            AutoLayoutCommand = new DelegateCommand(AutoLayoutNodes);
            UndoCommand = new DelegateCommand(() => GraphHistory.Undo(), () => GraphHistory.CanUndo);
            RedoCommand = new DelegateCommand(() => GraphHistory.Redo(), () => GraphHistory.CanRedo);
            SaveCommand = new DelegateCommand(SaveGraph);
            SaveAsCommand = new DelegateCommand(SaveAsGraph);
            LoadCommand = new DelegateCommand(LoadGraph);
            ClearCommand = new DelegateCommand(ClearAll);

            // 启动时设置 Nodify 深色主题
            Nodify.ThemeManager.SetTheme("Dark");
            DeleteSelectedCommand = new DelegateCommand(DeleteSelected);
            DeleteConnectionCommand = new DelegateCommand<ConnectionViewModel>(DeleteConnection);

            AddVariableCommand = new DelegateCommand(AddVariable);
            RemoveVariableCommand = new DelegateCommand(RemoveVariable, () => SelectedVariable != null);
            HardwareConfigCommand = new DelegateCommand(OpenHardwareConfig);

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

            // 特殊处理：FunctionNode 需要根据 DisplayName 设置运算类型
            if (item.NodeType == "Function")
            {
                var fn = CreateFunctionNode(item.DisplayName, item.DefaultTitle, graphPosition);
                AddNode(fn);
                return;
            }

            // 优先使用 NodeFactory（覆盖所有 [Node] 注册 + 内置类型）
            var factory = new NodeFactory();
            NodeViewModel? node = factory.CreateNode(item.NodeType);

            if (node != null)
            {
                node.Location = graphPosition;
                node.Title = item.DefaultTitle;
                AddNode(node);
            }
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

            // 校验 5：防止形成循环依赖（等待信号/循环判断节点允许回环）
            bool isLoopNode = target.ParentNode is Shell.Models.Nodes.Flow.WaitSignalNodeViewModel
                           || target.ParentNode is Shell.Models.Nodes.Flow.WhileNodeViewModel;
            if (!isLoopNode && FlowExecutor.WouldCreateCycle(Nodes.ToList(), Connections.ToList(), source, target))
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
        /// 使用 FlowExecutor 执行整张图的流程。
        /// </summary>
        private async Task ExecuteAllAsync()
        {
            if (IsRunning) return;  // 防止重复启动

            ExecutionError = null;
            IsRunning = true;
            // 强制刷新停止按钮可用状态
            ((Prism.Commands.DelegateCommand)StopCommand).RaiseCanExecuteChanged();
            // 清理上一次残留的 _cts，然后创建新的
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                var snapshotNodes = Nodes.ToList();
                var snapshotConns = Connections.ToList();

                // 启动前：根据模式初始化条件变量
                foreach (var node in snapshotNodes.OfType<WhileNodeViewModel>())
                {
                    if (!string.IsNullOrEmpty(node.ConditionVariable))
                    {
                        var v = VariableManager.GetVariable(node.ConditionVariable);
                        if (v == null)
                        {
                            bool initValue = node.LoopMode != "等待触发";
                            v = VariableManager.AddVariable(node.ConditionVariable, "Boolean",
                                VariantValue.FromBoolean(initValue));
                        }
                        else if (node.LoopMode != "等待触发")
                        {
                            v.Value = VariantValue.FromBoolean(true);
                        }
                    }
                }
                // 等待信号节点：自动创建信号变量（默认 false，等待外部触发）
                foreach (var node in snapshotNodes.OfType<WaitSignalNodeViewModel>())
                {
                    if (!string.IsNullOrEmpty(node.SignalVariable))
                    {
                        var v = VariableManager.GetVariable(node.SignalVariable);
                        if (v == null)
                            v = VariableManager.AddVariable(node.SignalVariable, "Boolean",
                                VariantValue.FromBoolean(false));
                    }
                }

                var result = await Task.Run(() =>
                    _flowExecutor.RunAsync(snapshotNodes, snapshotConns, _cts.Token));

                if (!result.Success)
                {
                    ExecutionError = result.ErrorMessage;
                    if (result.WasCancelled)
                        ExecutionLogger.Info("执行器", "流程已停止");
                    else
                        ExecutionLogger.Error("执行器", result.ErrorMessage);
                }
                else
                {
                    ExecutionLogger.Success("执行器", $"流程执行完成，共 {result.ExecutedNodeCount} 个节点");
                }
            }
            catch (Exception ex)
            {
                ExecutionError = ex.Message;
                ExecutionLogger.Error("执行器", $"执行异常：{ex.Message}");
            }
            finally
            {
                IsRunning = false;
                // 只清除遗留的 Running 节点（Error 保留供用户查看）
                foreach (var n in Nodes)
                {
                    if (n.State == ExecutionState.Running)
                    {
                        n.State = ExecutionState.Idle;
                        n.ExecutionTime = null;
                    }
                }
                ((Prism.Commands.DelegateCommand)StopCommand).RaiseCanExecuteChanged();
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void StopFlow()
        {
            ExecutionLogger.Warning("执行器", "⏹ 收到停止指令，准备取消...");
            // 「立即循环」模式 WhileNode：设变量为 false 使循环退出
            foreach (var node in Nodes.OfType<WhileNodeViewModel>())
            {
                if (!string.IsNullOrEmpty(node.ConditionVariable) && node.LoopMode != "等待触发")
                {
                    var v = VariableManager.GetVariable(node.ConditionVariable);
                    if (v != null)
                        v.Value = VariantValue.FromBoolean(false);
                }
            }
            // 等待信号 / WhileNode「等待触发」模式：不修改变量，由 CancellationToken 中断
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                ExecutionLogger.Warning("执行器", "⏹ CancellationToken 已取消并释放");
            }
            else
            {
                // 流程已自行退出：手动清除残留的 Error / Running 状态，耗时清零
                ExecutionLogger.Warning("执行器", "⏹ _cts 为 null（流程已退出），清除节点错误状态...");
                foreach (var n in Nodes)
                {
                    if (n.State == ExecutionState.Running || n.State == ExecutionState.Error)
                    {
                        n.State = ExecutionState.Idle;
                        n.ExecutionTime = null;
                    }
                }
            }
            // 刷新按钮状态
            ((Prism.Commands.DelegateCommand)StopCommand).RaiseCanExecuteChanged();
        }

        // ── 图像对比模式 ──

        public bool IsCompareMode => SelectedNodes.Count == 2
            && SelectedNodes[0].GetType().GetProperty("Preview") != null
            && SelectedNodes[1].GetType().GetProperty("Preview") != null;

        public bool IsSingleMode => !IsCompareMode;

        public System.Windows.Media.ImageSource? SecondPreviewImage
        {
            get
            {
                if (!IsCompareMode) return null;
                return GetPreviewImageSource(SelectedNodes[1]);
            }
        }

        /// <summary>通过反射读取节点的 Preview.ImageSource（支持 ImagePreview 组件）。</summary>
        private static System.Windows.Media.ImageSource? GetPreviewImageSource(NodeViewModel node)
        {
            var previewProp = node.GetType().GetProperty("Preview");
            if (previewProp != null)
            {
                var preview = previewProp.GetValue(node);
                if (preview != null)
                {
                    var srcProp = preview.GetType().GetProperty("ImageSource");
                    return srcProp?.GetValue(preview) as System.Windows.Media.ImageSource;
                }
            }
            return null;
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
        /// 打开硬件配置窗口（轴参数 / IO 信号）。
        /// </summary>
        private void OpenHardwareConfig()
        {
            var win = new HardwareConfigWindow
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
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

        // ── 变量管理 ──

        private void AddVariable()
        {
            // 打开变量管理窗口进行新增
            VariableManagerDialog.Show(VariableManager, Application.Current.MainWindow);
        }

        private void RemoveVariable()
        {
            if (SelectedVariable != null)
            {
                VariableManager.RemoveVariable(SelectedVariable);
                SelectedVariable = null;
                ((DelegateCommand)RemoveVariableCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 保存当前图。如果已有保存/打开路径则直接覆写，否则弹出另存为对话框。
        /// </summary>
        private void SaveGraph()
        {
            if (_currentFilePath != null)
            {
                // 已有路径，直接覆写
                SaveToFile(_currentFilePath);
                return;
            }

            // 首次保存 → 弹出对话框选路径
            SaveAsGraph();
        }

        /// <summary>
        /// 另存为 — 始终弹出对话框选择路径。
        /// </summary>
        private void SaveAsGraph()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "流程图文件 (*.flow)|*.flow|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".flow",
                FileName = !string.IsNullOrEmpty(_currentFilePath)
                    ? System.IO.Path.GetFileName(_currentFilePath)
                    : "graph.flow"
            };

            if (dialog.ShowDialog() == true)
            {
                _currentFilePath = dialog.FileName;
                SaveToFile(_currentFilePath);
            }
        }

        /// <summary>将图序列化写入指定路径。</summary>
        private void SaveToFile(string filePath)
        {
            try
            {
                var json = _serializer.Serialize(Nodes.ToList(), Connections.ToList(), VariableManager);
                System.IO.File.WriteAllText(filePath, json);
                Growl.Success(new GrowlInfo
                {
                    Message = $"已保存到：{filePath}",
                    WaitTime = 3
                });
            }
            catch (Exception ex)
            {
                Growl.Error(new GrowlInfo
                {
                    Message = $"保存失败：{ex.Message}",
                    WaitTime = 3
                });
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
                    var (loadedNodes, connectionDatas, variableDatas) = _serializer.Deserialize(json);

                    // 清空当前图（先断连后清空，确保干净）
                    foreach (var conn in Connections.ToList())
                        RemoveConnectionInternal(conn);
                    Nodes.Clear();
                    GraphHistory.Clear();

                    // ── 先恢复变量定义 ──
                    VariableManager.Clear();
                    foreach (var vd in variableDatas)
                    {
                        var variable = new Variable
                        {
                            Name = vd.Name,
                            TypeName = vd.TypeName ?? "Double",
                            Description = vd.Description ?? ""
                        };
                        // 从字符串恢复变量值
                        if (!string.IsNullOrEmpty(vd.Value))
                            variable.ValueString = vd.Value;
                        VariableManager.Variables.Add(variable);
                    }

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

                    // 记录加载路径，后续"保存"可直接覆写
                    _currentFilePath = dialog.FileName;

                    Growl.Success(new GrowlInfo
                    {
                        Message = $"已加载：{_currentFilePath}",
                        WaitTime = 3
                    });
                }
                catch (Exception ex)
                {
                    Growl.Error(new GrowlInfo
                    {
                        Message = $"加载失败：{ex.Message}",
                        WaitTime = 3
                    });
                }
            }
        }
    }
}
