using System.Net.WebSockets;
using System.Text.Json;
using Haus.Hass;

namespace Haus.Ws;

public sealed class HassWebSocketClient(ITokenProvider tokens) : IHassWebSocketClient, IDisposable
{
    private ClientWebSocket? _ws;
    private int _messageId;

    public async Task<JsonElement> SendCommandAsync(object command, CancellationToken cancellationToken = default)
    {
        var ws = await EnsureConnectedAsync(cancellationToken);
        var id = Interlocked.Increment(ref _messageId);

        var payload = new Dictionary<string, object>((command as IDictionary<string, object>) ?? ToDictionary(command))
        {
            ["id"] = id
        };

        await SendAsync(ws, payload, cancellationToken);
        return await ReceiveResultAsync(ws, id, cancellationToken);
    }

    private async Task<ClientWebSocket> EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_ws is { State: WebSocketState.Open })
            return _ws;

        var (url, token) = await tokens.GetAccessTokenAsync(cancellationToken);
        var wsUrl = url.Replace("http://", "ws://").Replace("https://", "wss://") + "/api/websocket";

        _ws = new ClientWebSocket();
        await _ws.ConnectAsync(new Uri(wsUrl), cancellationToken);

        // auth_required
        await ReceiveAsync(_ws, cancellationToken);

        // authenticate
        await SendAsync(_ws, new { type = "auth", access_token = token }, cancellationToken);
        var authResp = await ReceiveAsync(_ws, cancellationToken);
        if (authResp.GetProperty("type").GetString() != "auth_ok")
            throw new InvalidOperationException("WebSocket authentication failed.");

        return _ws;
    }

    private static async Task SendAsync(ClientWebSocket ws, object data, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(data);
        await ws.SendAsync(json, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static async Task<JsonElement> ReceiveAsync(ClientWebSocket ws, CancellationToken cancellationToken)
    {
        var buffer = new byte[65536];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await ws.ReceiveAsync(buffer, cancellationToken);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        ms.Position = 0;
        var doc = JsonDocument.Parse(ms);
        return doc.RootElement.Clone();
    }

    private static async Task<JsonElement> ReceiveResultAsync(ClientWebSocket ws, int expectedId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var msg = await ReceiveAsync(ws, cancellationToken);

            if (!msg.TryGetProperty("id", out var idProp) || idProp.GetInt32() != expectedId)
                continue;

            if (msg.TryGetProperty("success", out var success) && !success.GetBoolean())
            {
                var error = msg.TryGetProperty("error", out var err)
                    ? err.GetProperty("message").GetString() : "Unknown error";
                throw new InvalidOperationException(error);
            }

            return msg.TryGetProperty("result", out var r) ? r.Clone() : default;
        }
    }

    private static Dictionary<string, object> ToDictionary(object obj)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(obj);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];
    }

    public void Dispose() => _ws?.Dispose();
}
