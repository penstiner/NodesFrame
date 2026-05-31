using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shell.Models;
using Shell.Models.Attributes;
using Shell.Views;
using Nodify.UndoRedo;

namespace Shell.Services
{
    public class NodeDialogService : INodeDialogService
    {
        public bool ShowDialog(NodeViewModel node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            // —— 撤销支持：捕获编辑前快照 ——
            var beforeSnapshot = CaptureProperties(node);

            var window = new NodeEditorWindow(node)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            var result = window.ShowDialog();
            bool confirmed = result == true;

            // —— 撤销支持：若确认且有变更，记录撤销操作 ——
            if (confirmed)
            {
                var afterSnapshot = CaptureProperties(node);
                if (!SnapshotsEqual(beforeSnapshot, afterSnapshot))
                {
                    var undoSnapshot = beforeSnapshot;
                    var redoSnapshot = afterSnapshot;
                    // 使用全局撤销栈记录属性变更
                    var history = NodeViewModel.GlobalGraphHistory;
                    history?.Record(
                        () => ApplySnapshot(node, redoSnapshot),
                        () => ApplySnapshot(node, undoSnapshot),
                        $"编辑 {node.Title}");
                }
            }

            return confirmed;
        }

        private static Dictionary<string, object?> CaptureProperties(NodeViewModel node)
        {
            var snapshot = new Dictionary<string, object?>();
            foreach (var prop in node.GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<NodePropertyAttribute>();
                if (attr == null) continue;
                snapshot[attr.Key ?? prop.Name] = prop.GetValue(node);
            }
            return snapshot;
        }

        private static bool SnapshotsEqual(Dictionary<string, object?> a, Dictionary<string, object?> b)
        {
            if (a.Count != b.Count) return false;
            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var bVal)) return false;
                if (!Equals(kv.Value, bVal)) return false;
            }
            return true;
        }

        private static void ApplySnapshot(NodeViewModel node, Dictionary<string, object?> snapshot)
        {
            foreach (var kv in snapshot)
            {
                var prop = node.GetType().GetProperties()
                    .FirstOrDefault(p =>
                    {
                        var a = p.GetCustomAttribute<NodePropertyAttribute>();
                        return a != null && (a.Key ?? p.Name) == kv.Key;
                    });
                if (prop != null && prop.CanWrite)
                {
                    try { prop.SetValue(node, kv.Value); }
                    catch { }
                }
            }
        }
    }
}
