using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using Nodify;
using Nodify.UndoRedo;
using Prism.Mvvm;
using Shell.Models;
using Shell.Services;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace Shell.ViewModels
{
    /// <summary>
    /// 单个流程文档的视图模型，包含节点、连接、历史、变量等。
    /// </summary>
    public class DocumentViewModel : BindableBase
    {
        private readonly IGraphSerializer _serializer;

        /// <summary>文档标题（显示在标签页上）。</summary>
        private string _title = "未命名";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>文档文件路径，null 表示尚未保存。</summary>
        public string? FilePath { get; set; }

        /// <summary>文档是否有未保存的修改。</summary>
        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        // ── 节点 / 连接 ──
        public ObservableCollection<NodeViewModel> Nodes { get; } = new();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new();
        public ObservableCollection<NodeViewModel> SelectedNodes { get; } = new();
        public PendingConnectionViewModel PendingConnection { get; }

        // ── 历史 / 变量 ──
        public ActionsHistory GraphHistory { get; } = new();
        public VariableManager VariableManager { get; } = new();

        // ── 命令 ──
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand DeleteConnectionCommand { get; }
        public ICommand DisconnectConnectorCommand { get; }

        // ── 运行相关（被 MainWindowViewModel 调用）──
        private string _executionError = string.Empty;
        public string ExecutionError
        {
            get => _executionError;
            set => SetProperty(ref _executionError, value);
        }

        public DocumentViewModel(IGraphSerializer serializer, string? loadPath = null)
        {
            _serializer = serializer;

            // ── 初始化命令 ──
            UndoCommand = new DelegateCommand(() => GraphHistory.Undo(), () => GraphHistory.CanUndo);
            RedoCommand = new DelegateCommand(() => GraphHistory.Redo(), () => GraphHistory.CanRedo);
            SaveCommand = new DelegateCommand(SaveGraph);
            SaveAsCommand = new DelegateCommand(SaveAsGraph);
            ClearCommand = new DelegateCommand(ClearAll);
            DeleteSelectedCommand = new DelegateCommand(DeleteSelected);
            DeleteConnectionCommand = new DelegateCommand<ConnectionViewModel>(DeleteConnection);

            GraphHistory.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GraphHistory.CanUndo) || e.PropertyName == nameof(GraphHistory.CanRedo))
                {
                    ((DelegateCommand)UndoCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)RedoCommand).RaiseCanExecuteChanged();
                }
            };

            // ── 选中节点变化 → 更新 Dirty 标记 ──
            SelectedNodes.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(SelectedPreviewImage));
                RaisePropertyChanged(nameof(SelectedImageInfo));
            };

            // ── PendingConnection ──
            PendingConnection = new PendingConnectionViewModel(this);
            DisconnectConnectorCommand = new DelegateCommand<ConnectorViewModel>(connector =>
            {
                var toRemove = Connections.Where(x => x.Source == connector || x.Target == connector).ToList();
                foreach (var conn in toRemove)
                {
                    var source = conn.Source;
                    var target = conn.Target;
                    GraphHistory.Record(
                        () => RemoveConnectionInternal(conn),
                        () => RestoreConnection(source, target),
                        "删除连接");
                    RemoveConnectionInternal(conn);
                }
            });

            // ── 如果指定了加载路径，立即加载 ──
            if (loadPath != null)
            {
                LoadFromFile(loadPath);
                Title = System.IO.Path.GetFileNameWithoutExtension(loadPath);
                FilePath = loadPath;
            }

            // 标记 Dirty 变化的通用订阅：节点/连接增删
            Nodes.CollectionChanged += (s, e) => IsDirty = true;
            Connections.CollectionChanged += (s, e) => IsDirty = true;

            // GraphHistory 记录操作也会导致 Dirty（通过 Record 间接触发）
        }

        // ═══════════════════════════════════════════
        //  选中节点图像预览（供 MainWindow 绑定）
        // ═══════════════════════════════════════════

        public ImageSource? SelectedPreviewImage
        {
            get
            {
                if (SelectedNodes.Count == 0) return null;
                var node = SelectedNodes[0];
                return GetPreviewImageSource(node);
            }
        }

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

        private static ImageSource? GetPreviewImageSource(NodeViewModel node)
        {
            try
            {
                // 节点有 Preview 属性，其 ImageSource 才是实际图片源
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
            }
            catch { /* 忽略反射错误 */ }
            return null;
        }

        // ═══════════════════════════════════════════
        //  节点/连接操作
        // ═══════════════════════════════════════════

        public void AddNode(NodeViewModel node)
        {
            Nodes.Add(node);
            GraphHistory.Record(
                () => Nodes.Add(node),
                () => Nodes.Remove(node),
                $"添加节点 {node.Title}");
        }

        public void RemoveNode(NodeViewModel node)
        {
            if (node == null) return;

            var relatedConns = Connections.Where(c => c.Source.ParentNode == node || c.Target.ParentNode == node).ToList();

            using (GraphHistory.Batch($"删除节点 {node.Title}"))
            {
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

                var removedNode = node;
                GraphHistory.Record(
                    () => Nodes.Remove(removedNode),
                    () => Nodes.Add(removedNode),
                    $"删除节点 {node.Title}");
                Nodes.Remove(node);
            }
        }

        public bool Connect(ConnectorViewModel source, ConnectorViewModel target)
        {
            if (source == null || target == null) return false;
            if (source == target) return false;

            // 通过所属节点集合判断方向：source 必须是输出，target 必须是输入
            bool sourceIsOutput = source.ParentNode?.Output.Contains(source) == true;
            bool targetIsInput = target.ParentNode?.Input.Contains(target) == true;
            if (!sourceIsOutput || !targetIsInput)
                return false;

            if (target.IsConnected)
                return false;

            if (Connections.Any(c => c.Source == source && c.Target == target))
                return false;

            var conn = new ConnectionViewModel(source, target);
            source.IsConnected = true;
            target.IsConnected = true;

            GraphHistory.Record(
                () =>
                {
                    source.IsConnected = true;
                    target.IsConnected = true;
                    Connections.Add(conn);
                },
                () =>
                {
                    source.IsConnected = false;
                    target.IsConnected = false;
                    Connections.Remove(conn);
                },
                "添加连接");

            Connections.Add(conn);
            return true;
        }

        private void RemoveConnectionInternal(ConnectionViewModel conn)
        {
            conn.Source.IsConnected = false;
            conn.Target.IsConnected = false;
            Connections.Remove(conn);
        }

        private void RestoreConnection(ConnectorViewModel source, ConnectorViewModel target)
        {
            source.IsConnected = true;
            target.IsConnected = true;
            var conn = new ConnectionViewModel(source, target);
            Connections.Add(conn);
        }

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

        public void DeleteSelected()
        {
            var selectedConns = Connections.Where(c => c.IsSelected).ToList();
            var selectedNodes = SelectedNodes.ToList();

            if (selectedConns.Count == 0 && selectedNodes.Count == 0)
                return;

            using (GraphHistory.Batch("删除选中项"))
            {
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

                foreach (var node in selectedNodes)
                {
                    RemoveNode(node);
                }
            }

            SelectedNodes.Clear();
        }

        public void ClearAll()
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
            ExecutionError = string.Empty;
        }

        // ═══════════════════════════════════════════
        //  保存 / 加载
        // ═══════════════════════════════════════════

        public void SaveGraph()
        {
            if (FilePath != null)
            {
                SaveToFile(FilePath);
                return;
            }
            SaveAsGraph();
        }

        public void SaveAsGraph()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "流程图文件 (*.flow)|*.flow|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".flow",
                FileName = !string.IsNullOrEmpty(FilePath)
                    ? System.IO.Path.GetFileName(FilePath)
                    : "graph.flow"
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                Title = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                SaveToFile(FilePath);
            }
        }

        private void SaveToFile(string filePath)
        {
            try
            {
                var json = _serializer.Serialize(Nodes.ToList(), Connections.ToList(), VariableManager);
                System.IO.File.WriteAllText(filePath, json);
                IsDirty = false;
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
        /// 从文件加载内容替换当前文档内容。
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            try
            {
                var json = System.IO.File.ReadAllText(filePath);
                var (loadedNodes, connectionDatas, variableDatas) = _serializer.Deserialize(json);

                // 清空当前
                foreach (var conn in Connections.ToList())
                    RemoveConnectionInternal(conn);
                Nodes.Clear();
                GraphHistory.Clear();

                // 恢复变量
                VariableManager.Clear();
                foreach (var vd in variableDatas)
                {
                    var variable = new Variable
                    {
                        Name = vd.Name,
                        TypeName = vd.TypeName ?? "Double",
                        Description = vd.Description ?? ""
                    };
                    if (!string.IsNullOrEmpty(vd.Value))
                        variable.ValueString = vd.Value;
                    VariableManager.Variables.Add(variable);
                }

                // 预标记连接
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

                foreach (var node in loadedNodes)
                    Nodes.Add(node);

                var nodeDict = Nodes.ToDictionary(n => n.Id);
                int connCount = 0;
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

                FilePath = filePath;
                Title = System.IO.Path.GetFileNameWithoutExtension(filePath);
                IsDirty = false;

                Growl.Success(new GrowlInfo
                {
                    Message = $"已加载：{filePath}",
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
