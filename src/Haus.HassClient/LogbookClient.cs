
namespace Haus.HassClient;

public interface ILogbookClient
{
    Task<IReadOnlyList<LogbookEntry>> ListAsync(
        DateTimeOffset start,
        string? entityId = null,
        DateTimeOffset? end = null,
        CancellationToken cancellationToken = default);
}

internal sealed class LogbookClient(IHassApiClient api) : ILogbookClient
{
    public Task<IReadOnlyList<LogbookEntry>> ListAsync(
        DateTimeOffset start,
        string? entityId = null,
        DateTimeOffset? end = null,
        CancellationToken cancellationToken = default) =>
        api.ListLogbookEntriesAsync(start, entityId, end, cancellationToken);
}
