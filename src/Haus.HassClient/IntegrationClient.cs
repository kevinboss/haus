using System.Text.Json;

namespace Haus.HassClient;

public interface IIntegrationClient
{
    Task<IReadOnlyList<ConfigEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> InitOptionsAsync(string entryId, CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> ConfigureOptionsAsync(string flowId, object userInput, CancellationToken cancellationToken = default);
    Task AbortOptionsAsync(string flowId, CancellationToken cancellationToken = default);
    Task<ConfigEntryOperationResult> ReloadAsync(string entryId, CancellationToken cancellationToken = default);
    Task<ConfigEntryOperationResult> SetEnabledAsync(string entryId, bool enabled, CancellationToken cancellationToken = default);
    Task<ConfigEntryOperationResult> RemoveAsync(string entryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConfigFlowProgress>> ListInProgressFlowsAsync(CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> StartReconfigureAsync(string handler, string entryId, CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> GetFlowAsync(string flowId, CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> SubmitFlowAsync(string flowId, object userInput, CancellationToken cancellationToken = default);
    Task AbortFlowAsync(string flowId, CancellationToken cancellationToken = default);
    Task<JsonElement> GetDiagnosticsAsync(string entryId, CancellationToken cancellationToken = default);
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

    public Task<ConfigEntryOperationResult> ReloadAsync(string entryId, CancellationToken cancellationToken = default) =>
        rest.ReloadConfigEntryAsync(entryId, cancellationToken);

    public Task<ConfigEntryOperationResult> SetEnabledAsync(string entryId, bool enabled, CancellationToken cancellationToken = default) =>
        ws.SetConfigEntryDisabledAsync(entryId, !enabled, cancellationToken);

    public Task<ConfigEntryOperationResult> RemoveAsync(string entryId, CancellationToken cancellationToken = default) =>
        rest.RemoveConfigEntryAsync(entryId, cancellationToken);

    public Task<IReadOnlyList<ConfigFlowProgress>> ListInProgressFlowsAsync(CancellationToken cancellationToken = default) =>
        ws.ListFlowsInProgressAsync(cancellationToken);

    public Task<OptionsFlowStep> StartReconfigureAsync(string handler, string entryId, CancellationToken cancellationToken = default) =>
        rest.StartConfigFlowAsync(handler, entryId, cancellationToken);

    public Task<OptionsFlowStep> GetFlowAsync(string flowId, CancellationToken cancellationToken = default) =>
        rest.GetConfigFlowAsync(flowId, cancellationToken);

    public Task<OptionsFlowStep> SubmitFlowAsync(string flowId, object userInput, CancellationToken cancellationToken = default) =>
        rest.ConfigureConfigFlowAsync(flowId, userInput, cancellationToken);

    public Task AbortFlowAsync(string flowId, CancellationToken cancellationToken = default) =>
        rest.AbortConfigFlowAsync(flowId, cancellationToken);

    public Task<JsonElement> GetDiagnosticsAsync(string entryId, CancellationToken cancellationToken = default) =>
        rest.GetConfigEntryDiagnosticsAsync(entryId, cancellationToken);
}
