using Haus.Auth;
using HassClient.WS;
using Microsoft.Extensions.Logging;

namespace Haus.Connection;

public sealed class HassConnection(IAuthService authService, ILogger<HassConnection> logger) : IHassConnection
{
    private readonly HassWSApi _client = new();

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public event Action<ConnectionState>? StateChanged;

    public HassWSApi Client => _client;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        SetState(ConnectionState.Connecting);

        var (url, token) = await authService.GetAccessTokenAsync(cancellationToken);
        var connectionParams = ConnectionParameters.CreateFromInstanceBaseUrl(url, token);

        logger.LogInformation("Connecting to Home Assistant at {Url}", url);
        await _client.ConnectAsync(connectionParams);

        SetState(ConnectionState.Connected);
        logger.LogInformation("Connected to Home Assistant");
    }

    public async Task DisconnectAsync()
    {
        await _client.CloseAsync();
        SetState(ConnectionState.Disconnected);
    }

    private void SetState(ConnectionState state)
    {
        State = state;
        StateChanged?.Invoke(state);
    }
}
