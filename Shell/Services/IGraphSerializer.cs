using System.Collections.Generic;
using Shell.Models;

namespace Shell.Services
{
    /// <summary>序列化连接数据 DTO</summary>
    public class ConnectionData
    {
        public Guid SourceNodeId { get; set; }
        public int SourceConnectorIndex { get; set; }
        public Guid TargetNodeId { get; set; }
        public int TargetConnectorIndex { get; set; }
    }

    /// <summary>变量序列化 DTO</summary>
    public class VariableData
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }

    /// <summary>属性绑定序列化 DTO</summary>
    public class PropertyBindingData
    {
        public bool IsBound { get; set; }
        public string VariableName { get; set; } = "";
    }

    public interface IGraphSerializer
    {
        string Serialize(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections, VariableManager variableManager = null);
        (List<NodeViewModel> nodes, List<ConnectionData> connections, List<VariableData> variables) Deserialize(string json);
    }
}
