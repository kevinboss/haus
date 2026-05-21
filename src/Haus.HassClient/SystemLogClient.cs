
namespace Haus.HassClient;

public interface ISystemLogClient
{
    Task<IReadOnlyList<SystemLogEntry>> ListAsync(CancellationToken cancellationToken = default);
}

internal sealed class SystemLogClient(IHassWebSocketClient ws) : ISystemLogClient
{
    public Task<IReadOnlyList<SystemLogEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListSystemLogAsync(cancellationToken);
}
