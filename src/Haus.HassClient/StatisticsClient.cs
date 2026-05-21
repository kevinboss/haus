
namespace Haus.HassClient;

public interface IStatisticsClient
{
    Task<IReadOnlyDictionary<string, IReadOnlyList<StatisticsRow>>> GetDuringPeriodAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<string> statisticIds,
        string period,
        CancellationToken cancellationToken = default);
}

internal sealed class StatisticsClient(IHassWebSocketClient ws) : IStatisticsClient
{
    public Task<IReadOnlyDictionary<string, IReadOnlyList<StatisticsRow>>> GetDuringPeriodAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<string> statisticIds,
        string period,
        CancellationToken cancellationToken = default) =>
        ws.GetStatisticsDuringPeriodAsync(start, end, statisticIds, period, cancellationToken);
}
