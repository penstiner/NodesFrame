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

    public interface IGraphSerializer
    {
        string Serialize(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections);
        (List<NodeViewModel> nodes, List<ConnectionData> connections) Deserialize(string json);
    }
}
