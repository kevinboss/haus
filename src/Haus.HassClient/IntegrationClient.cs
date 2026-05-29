namespace Haus.HassClient;

public interface IIntegrationClient
{
    Task<IReadOnlyList<ConfigEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> InitOptionsAsync(string entryId, CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> ConfigureOptionsAsync(string flowId, object userInput, CancellationToken cancellationToken = default);
    Task AbortOptionsAsync(string flowId, CancellationToken cancellationToken = default);
}

internal sealed class IntegrationClient(IHassWebSocketClient ws, IHassApiClient rest) : IIntegrationClient
{
    public Task<IReadOnlyList<ConfigEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListConfigEntriesAsync(cancellationToken);

    public Task<OptionsFlowStep> InitOptionsAsync(string entryId, CancellationToken cancellationToken = default) =>
        rest.InitOptionsFlowAsync(entryId, cancellationToken);

    public Task<OptionsFlowStep> ConfigureOptionsAsync(string flowId, object userInput, CancellationToken cancellationToken = default) =>
        rest.ConfigureOptionsFlowAsync(flowId, userInput, cancellationToken);

    public Task AbortOptionsAsync(string flowId, CancellationToken cancellationToken = default) =>
        rest.AbortOptionsFlowAsync(flowId, cancellationToken);
}
