using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Haus.HassClient;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the typed Home Assistant client and the underlying REST + WebSocket transports.
    /// Caller is responsible for registering an <see cref="ITokenProvider"/> implementation.
    /// </summary>
    public static IServiceCollection AddHassClient(this IServiceCollection services)
    {
        services.AddSingleton<IHassApiClient, HassApiClient>();
        services.AddSingleton<IHassWebSocketClient, HassWebSocketClient>();
        services.AddSingleton<IHassClient, HassClient>();
        return services;
    }
}
