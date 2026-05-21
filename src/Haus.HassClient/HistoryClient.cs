
namespace Haus.HassClient;

public interface IHistoryClient
{
    Task<IReadOnlyList<IReadOnlyList<HistoryState>>> GetAsync(
        DateTimeOffset start,
        IEnumerable<string> entityIds,
        DateTimeOffset? end = null,
        bool includeAttributes = true,
        CancellationToken cancellationToken = default);
}

internal sealed class HistoryClient(IHassApiClient api) : IHistoryClient
{
    public Task<IReadOnlyList<IReadOnlyList<HistoryState>>> GetAsync(
        DateTimeOffset start,
        IEnumerable<string> entityIds,
        DateTimeOffset? end = null,
        bool includeAttributes = true,
        CancellationToken cancellationToken = default) =>
        api.GetHistoryAsync(start, entityIds, end, includeAttributes, cancellationToken);
}
