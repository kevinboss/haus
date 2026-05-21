using System.Text.Json;

namespace Haus.Ws;

public interface IHassWebSocketClient
{
    Task<JsonElement> SendCommandAsync(object command, CancellationToken cancellationToken = default);
}
