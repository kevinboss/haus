
namespace Haus.HassClient;

public interface ILabelRegistryClient
{
    Task<IReadOnlyList<LabelEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<LabelEntry?> GetAsync(string labelId, CancellationToken cancellationToken = default);
    Task<LabelEntry> CreateAsync(NewLabel label, CancellationToken cancellationToken = default);
    Task UpdateAsync(string labelId, LabelUpdate update, CancellationToken cancellationToken = default);
    Task DeleteAsync(string labelId, CancellationToken cancellationToken = default);
}

internal sealed class LabelRegistryClient(IHassWebSocketClient ws) : ILabelRegistryClient
{
    public Task<IReadOnlyList<LabelEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListLabelRegistryAsync(cancellationToken);

    // HA exposes no label_registry/get endpoint — resolve against the list.
    public async Task<LabelEntry?> GetAsync(string labelId, CancellationToken cancellationToken = default)
    {
        var labels = await ws.ListLabelRegistryAsync(cancellationToken);
        return labels.FirstOrDefault(l => l.LabelId == labelId);
    }

    public Task<LabelEntry> CreateAsync(NewLabel label, CancellationToken cancellationToken = default) =>
        ws.CreateLabelAsync(label, cancellationToken);

    public Task UpdateAsync(string labelId, LabelUpdate update, CancellationToken cancellationToken = default) =>
        ws.UpdateLabelAsync(labelId, update, cancellationToken);

    public Task DeleteAsync(string labelId, CancellationToken cancellationToken = default) =>
        ws.DeleteLabelAsync(labelId, cancellationToken);
}
