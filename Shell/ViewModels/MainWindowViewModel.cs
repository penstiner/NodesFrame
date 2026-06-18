using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        // ═══════════════════════════════════════════
        //  多文档管理
        // ═══════════════════════════════════════════

        /// <summary>所有打开的文档。</summary>
        public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>();

        /// <summary>当前激活的文档。</summary>
        private DocumentViewModel? _activeDocument;
        public DocumentViewModel? ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (SetProperty(ref _activeDocument, value))
                {
                    OnActiveDocumentChanged();
                }
            }
        }

        /// <summary>当前是否有打开的文档。</summary>
        public bool HasDocument => ActiveDocument != null;

        private DocumentViewModel? _subscribedDoc;

        private void OnActiveDocumentChanged()
        {
            // 取消旧文档的 SelectedNodes 订阅
            if (_subscribedDoc != null)
                _subscribedDoc.SelectedNodes.CollectionChanged -= OnSelectedNodesChanged;

            var doc = ActiveDocument;
            if (doc != null)
            {
                NodeViewModel.GlobalVariableManager = doc.VariableManager;
                NodeViewModel.GlobalGraphHistory = doc.GraphHistory;
                _flowExecutor.VariableManager = doc.VariableManager;

                // 订阅新文档的 SelectedNodes 变化，同步刷新图像预览
                doc.SelectedNodes.CollectionChanged += OnSelectedNodesChanged;
                _subscribedDoc = doc;
            }
            else
            {
                NodeViewModel.GlobalVariableManager = null;
                NodeViewModel.GlobalGraphHistory = null;
                _flowExecutor.VariableManager = null;
                _subscribedDoc = null;
            }

            // 刷新绑定到 ActiveDocument 子属性的命令
            ((DelegateCommand)UndoCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)RedoCommand)?.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(HasDocument));
            RaisePropertyChanged(nameof(ActiveDocument));
            RaisePropertyChanged(nameof(VariableManager));
            RefreshPreviewProperties();
        }

        private void OnSelectedNodesChanged(object? s, NotifyCollectionChangedEventArgs e)
        {
            // 确保在 UI 线程上刷新预览属性
            if (Application.Current?.Dispatcher.CheckAccess() == false)
            {
                Application.Current.Dispatcher.BeginInvoke(() => RefreshPreviewProperties());
            }
            else
            {
                RefreshPreviewProperties();
            }
        }

        private void RefreshPreviewProperties()
        {
            RaisePropertyChanged(nameof(SelectedPreviewImage));
            RaisePropertyChanged(nameof(SelectedImageInfo));
            RaisePropertyChanged(nameof(IsCompareMode));
            RaisePropertyChanged(nameof(IsSingleMode));
            RaisePropertyChanged(nameof(SecondPreviewImage));
            RaisePropertyChanged(nameof(SecondImageInfo));
        }

        // ── 文档命令 ──
        public ICommand NewDocumentCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand CloseDocumentWithParamCommand { get; }

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

        // 撤销 / 重做（委托到 ActiveDocument）
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        // 保存 / 加载（委托到 ActiveDocument）
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveAsCommand { get; }

        // 清空编辑器命令（委托到 ActiveDocument）
        public ICommand ClearCommand { get; }

        // 删除选中项命令（委托到 ActiveDocument）
        public ICommand DeleteSelectedCommand { get; }

        // 删除连接线命令（右键菜单，委托到 ActiveDocument）
        public ICommand DeleteConnectionCommand { get; }

        // 变量管理命令（操作 ActiveDocument 的变量）
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

        // ── 以下属性委托到 ActiveDocument ──

        /// <summary>当前文档的选中节点（用于图像预览）。</summary>
        public ObservableCollection<NodeViewModel>? SelectedNodes => ActiveDocument?.SelectedNodes;

        /// <summary>安全获取选中节点的预览图像。</summary>
        public ImageSource? SelectedPreviewImage => ActiveDocument?.SelectedPreviewImage;

        /// <summary>安全获取选中节点的图像信息。</summary>
        public string SelectedImageInfo => ActiveDocument?.SelectedImageInfo ?? "点击选择一个图像节点查看预览";

        /// <summary>当前文档的变量管理器（委托到 ActiveDocument，供 VariableManagerPanel 绑定）。</summary>
        public VariableManager? VariableManager => ActiveDocument?.VariableManager;

        /// <summary>工具箱。</summary>
        public ToolBoxViewModel ToolBox { get; } = new ToolBoxViewModel();

        private string _executionError = string.Empty;
        public string ExecutionError
        {
            get => _executionError;
            set => SetProperty(ref _executionError, value);
        }

        public MainWindowViewModel(IGraphSerializer serializer)
        {
            _serializer = serializer;

            // ── 先绑定所有命令（确保 OnActiveDocumentChanged 能安全访问它们）──
            NewDocumentCommand = new DelegateCommand(NewDocument);
            CloseDocumentCommand = new DelegateCommand(CloseCurrentDocument);
            CloseDocumentWithParamCommand = new DelegateCommand<DocumentViewModel>(CloseDocument);

            ExecuteCommand = new Nodify.AsyncDelegateCommand(ExecuteAllAsync);
            StopCommand = new DelegateCommand(StopFlow);
            AutoLayoutCommand = new DelegateCommand(AutoLayoutNodes);
            UndoCommand = new DelegateCommand(
                () => ActiveDocument?.GraphHistory.Undo(),
                () => ActiveDocument?.GraphHistory.CanUndo ?? false);
            RedoCommand = new DelegateCommand(
                () => ActiveDocument?.GraphHistory.Redo(),
                () => ActiveDocument?.GraphHistory.CanRedo ?? false);
            SaveCommand = new DelegateCommand(() => ActiveDocument?.SaveGraph());
            SaveAsCommand = new DelegateCommand(() => ActiveDocument?.SaveAsGraph());
            LoadCommand = new DelegateCommand(LoadGraph);
            ClearCommand = new DelegateCommand(() => ActiveDocument?.ClearAll());
            DeleteSelectedCommand = new DelegateCommand(() => ActiveDocument?.DeleteSelected());
            DeleteConnectionCommand = new DelegateCommand<ConnectionViewModel>(conn =>
                ActiveDocument?.DeleteConnectionCommand?.Execute(conn));

            AddVariableCommand = new DelegateCommand(AddVariable);
            RemoveVariableCommand = new DelegateCommand(RemoveVariable, () => SelectedVariable != null);
            HardwareConfigCommand = new DelegateCommand(OpenHardwareConfig);

            // 启动时设置 Nodify 深色主题
            Nodify.ThemeManager.SetTheme("Dark");

            // ── 再创建初始空白文档（此时所有命令已就绪）──
            NewDocument();
        }

        // ═══════════════════════════════════════════
        //  文档管理
        // ═══════════════════════════════════════════

        /// <summary>创建新的空白文档。</summary>
        public void NewDocument()
        {
            var doc = new DocumentViewModel(_serializer);
            var count = Documents.Count(d => d.Title.StartsWith("未命名"));
            doc.Title = count > 0 ? $"未命名 {count + 1}" : "未命名";
            Documents.Add(doc);
            ActiveDocument = doc;
            SubscribeDocumentHistory(doc);
        }

        /// <summary>关闭指定文档。</summary>
        public void CloseDocument(DocumentViewModel? doc)
        {
            if (doc == null) return;

            // 如果有未保存的修改，提示保存（略，可根据需要添加确认对话框）

            var wasActive = doc == ActiveDocument;
            Documents.Remove(doc);

            if (wasActive)
            {
                // 切换到下一个可用的文档
                ActiveDocument = Documents.LastOrDefault();
                if (ActiveDocument != null)
                    SubscribeDocumentHistory(ActiveDocument);
            }
        }

        private void CloseCurrentDocument() => CloseDocument(ActiveDocument);

        /// <summary>订阅文档历史变化以刷新撤销/重做按钮。</summary>
        private void SubscribeDocumentHistory(DocumentViewModel doc)
        {
            // 取消旧订阅
            if (_historySubscription != null)
            {
                _historySubscription.PropertyChanged -= OnHistoryPropertyChanged;
            }
            _historySubscription = doc.GraphHistory;
            _historySubscription.PropertyChanged += OnHistoryPropertyChanged;
        }

        private ActionsHistory? _historySubscription;

        private void OnHistoryPropertyChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ActionsHistory.CanUndo) || e.PropertyName == nameof(ActionsHistory.CanRedo))
            {
                ((DelegateCommand)UndoCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RedoCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 向当前文档添加节点（委托到 ActiveDocument）。
        /// </summary>
        public void AddNode(NodeViewModel node)
        {
            ActiveDocument?.AddNode(node);
        }

        /// <summary>
        /// 从工具箱拖放创建节点，添加到当前激活的文档。
        /// </summary>
        public void AddNodeFromToolBox(ToolBoxItem item, Point graphPosition)
        {
            var doc = ActiveDocument;
            if (doc == null || item == null) return;

            if (item.NodeType == "Function")
            {
                var fn = CreateFunctionNode(item.DisplayName, item.DefaultTitle, graphPosition);
                doc.AddNode(fn);
                return;
            }

            var factory = new NodeFactory();
            NodeViewModel? node = factory.CreateNode(item.NodeType);

            if (node != null)
            {
                node.Location = graphPosition;
                node.Title = item.DefaultTitle;
                doc.AddNode(node);
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
        /// 移除节点（委托到 ActiveDocument）。
        /// </summary>
        public void RemoveNode(NodeViewModel node)
        {
            ActiveDocument?.RemoveNode(node);
        }

        /// <summary>
        /// 创建连接（委托到 ActiveDocument），含循环检测。
        /// </summary>
        public bool Connect(ConnectorViewModel source, ConnectorViewModel target)
        {
            var doc = ActiveDocument;
            if (doc == null || source == null || target == null) return false;
            if (source == target) return false;

            bool sourceIsOutput = source.ParentNode?.Output.Contains(source) == true;
            bool targetIsInput = target.ParentNode?.Input.Contains(target) == true;
            if (!sourceIsOutput || !targetIsInput) return false;

            if (target.IsConnected) return false;

            if (doc.Connections.Any(c => c.Source == source && c.Target == target))
                return false;

            bool isLoopNode = target.ParentNode is WaitSignalNodeViewModel
                           || target.ParentNode is WhileNodeViewModel;
            if (!isLoopNode && FlowExecutor.WouldCreateCycle(
                    doc.Nodes.ToList(), doc.Connections.ToList(), source, target))
            {
                ExecutionError = "无法创建连接：将形成循环依赖。";
                return false;
            }

            return doc.Connect(source, target);
        }

        /// <summary>
        /// 使用 FlowExecutor 执行整张图的流程。
        /// </summary>
        private async Task ExecuteAllAsync()
        {
            var doc = ActiveDocument;
            if (doc == null || IsRunning) return;

            ExecutionError = null;
            IsRunning = true;
            ((DelegateCommand)StopCommand).RaiseCanExecuteChanged();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                var snapshotNodes = doc.Nodes.ToList();
                var snapshotConns = doc.Connections.ToList();
                var varMgr = doc.VariableManager;

                // 启动前：初始化条件变量
                foreach (var node in snapshotNodes.OfType<WhileNodeViewModel>())
                {
                    if (!string.IsNullOrEmpty(node.ConditionVariable))
                    {
                        var v = varMgr.GetVariable(node.ConditionVariable);
                        if (v == null)
                        {
                            bool initValue = node.LoopMode != "等待触发";
                            v = varMgr.AddVariable(node.ConditionVariable, "Boolean",
                                VariantValue.FromBoolean(initValue));
                        }
                        else if (node.LoopMode != "等待触发")
                        {
                            v.Value = VariantValue.FromBoolean(true);
                        }
                    }
                }
                foreach (var node in snapshotNodes.OfType<WaitSignalNodeViewModel>())
                {
                    var name = node.ResolvedSignalName;
                    if (!string.IsNullOrEmpty(name))
                    {
                        var v = varMgr.GetVariable(name);
                        if (v == null)
                            v = varMgr.AddVariable(name, "Boolean",
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
                foreach (var n in doc.Nodes)
                {
                    if (n.State == ExecutionState.Running)
                    {
                        n.State = ExecutionState.Idle;
                        n.ExecutionTime = null;
                    }
                }
                ((DelegateCommand)StopCommand).RaiseCanExecuteChanged();
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void StopFlow()
        {
            var doc = ActiveDocument;
            if (doc == null) return;

            ExecutionLogger.Warning("执行器", "⏹ 收到停止指令，准备取消...");
            foreach (var node in doc.Nodes.OfType<WhileNodeViewModel>())
            {
                if (!string.IsNullOrEmpty(node.ConditionVariable) && node.LoopMode != "等待触发")
                {
                    var v = doc.VariableManager.GetVariable(node.ConditionVariable);
                    if (v != null)
                        v.Value = VariantValue.FromBoolean(false);
                }
            }
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                ExecutionLogger.Warning("执行器", "⏹ CancellationToken 已取消并释放");
                // 立即重置所有节点状态与执行耗时，避免停止后 UI 仍显示旧的执行结果
                foreach (var n in doc.Nodes)
                {
                    n.State = ExecutionState.Idle;
                    n.ExecutionTime = null;
                }
            }
            else
            {
                ExecutionLogger.Warning("执行器", "⏹ _cts 为 null，清除节点错误状态...");
                foreach (var n in doc.Nodes)
                {
                    if (n.State == ExecutionState.Running || n.State == ExecutionState.Error)
                    {
                        n.State = ExecutionState.Idle;
                        n.ExecutionTime = null;
                    }
                }
            }
            ((DelegateCommand)StopCommand).RaiseCanExecuteChanged();
        }

        // ── 图像对比模式 ──

        public bool IsCompareMode => ActiveDocument?.SelectedNodes.Count == 2
            && ActiveDocument.SelectedNodes[0].GetType().GetProperty("Preview") != null
            && ActiveDocument.SelectedNodes[1].GetType().GetProperty("Preview") != null;

        public bool IsSingleMode => !IsCompareMode;

        public ImageSource? SecondPreviewImage
        {
            get
            {
                if (!IsCompareMode || ActiveDocument == null) return null;
                return GetPreviewImageSource(ActiveDocument.SelectedNodes[1]);
            }
        }

        private static ImageSource? GetPreviewImageSource(NodeViewModel node)
        {
            var previewProp = node.GetType().GetProperty("Preview");
            if (previewProp != null)
            {
                var preview = previewProp.GetValue(node);
                if (preview != null)
                {
                    var srcProp = preview.GetType().GetProperty("ImageSource");
                    return srcProp?.GetValue(preview) as ImageSource;
                }
            }
            return null;
        }

        public string SecondImageInfo
        {
            get
            {
                if (!IsCompareMode || ActiveDocument == null) return "";
                var node = ActiveDocument.SelectedNodes[1];
                var prop = node.GetType().GetProperty("ImageInfo");
                if (prop?.GetValue(node) is string s && !string.IsNullOrEmpty(s)) return s;
                return $"节点: {node.Title}";
            }
        }

        // ── 自动布局 ──

        private void AutoLayoutNodes()
        {
            var doc = ActiveDocument;
            if (doc == null || doc.Nodes.Count == 0) return;
            var list = doc.Nodes.ToList();

            var inDegree = new Dictionary<NodeViewModel, int>();
            var adjacency = new Dictionary<NodeViewModel, List<NodeViewModel>>();
            foreach (var n in list) { inDegree[n] = 0; adjacency[n] = new List<NodeViewModel>(); }
            foreach (var c in doc.Connections)
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
        /// 切换连接的选中状态（单选模式：选中一条时自动取消其他）。
        /// </summary>
        public void ToggleConnectionSelection(ConnectionViewModel conn)
        {
            var doc = ActiveDocument;
            if (doc == null || conn == null) return;

            bool wasSelected = conn.IsSelected;
            foreach (var c in doc.Connections)
                c.IsSelected = false;

            if (!wasSelected)
                conn.IsSelected = true;
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

        // ── 变量管理 ──

        private void AddVariable()
        {
            var doc = ActiveDocument;
            if (doc == null) return;
            VariableManagerDialog.Show(doc.VariableManager, Application.Current.MainWindow);
        }

        private void RemoveVariable()
        {
            var doc = ActiveDocument;
            if (doc == null || SelectedVariable == null) return;
            doc.VariableManager.RemoveVariable(SelectedVariable);
            SelectedVariable = null;
            ((DelegateCommand)RemoveVariableCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 打开文件加载为新的文档标签页。
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
                var doc = new DocumentViewModel(_serializer, dialog.FileName);
                Documents.Add(doc);
                ActiveDocument = doc;
                SubscribeDocumentHistory(doc);
            }
        }
    }
}
