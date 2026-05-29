using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shell.Models;

namespace Shell.Services
{
    public interface IGraphExecutor
    {
        GraphExecutor.ExecutionResult Execute(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections);
        Task<GraphExecutor.ExecutionResult> ExecuteAsync(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections, CancellationToken ct = default);
        bool WouldCreateCycle(IReadOnlyList<NodeViewModel> nodes, IReadOnlyList<ConnectionViewModel> connections, ConnectorViewModel source, ConnectorViewModel target);
    }
}
