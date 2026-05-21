using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.History;

public sealed class HistoryGetCommand(IAuthService auth, IHassApiClient api, IHassWebSocketClient ws)
    : HausCommand<HistoryGetCommand.Settings>(auth)
{
    private static readonly string[] StatisticsPeriods = ["5minute", "hour", "day", "week", "month"];

    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity to fetch state history for")]
        public required string EntityId { get; init; }

        [CommandOption("-s|--since <DURATION>")]
        [Description("How far back to look (e.g. 30m, 1h, 2d). Default: 1h.")]
        public string Since { get; init; } = "1h";

        [CommandOption("-u|--until <ISO_TIMESTAMP>")]
        [Description("End timestamp (ISO 8601). Default: now.")]
        public string? Until { get; init; }

        [CommandOption("--with-attributes")]
        [Description("Include full state attributes (default omits them for compactness)")]
        public bool WithAttributes { get; init; }

        [CommandOption("--statistics <PERIOD>")]
        [Description("Use recorder statistics instead of raw state changes. Period: 5minute, hour, day, week, month.")]
        public string? Statistics { get; init; }

        public override ValidationResult Validate()
        {
            try { _ = DurationParser.Parse(Since); }
            catch (FormatException ex) { return ValidationResult.Error(ex.Message); }

            if (Until is not null && !DateTimeOffset.TryParse(Until, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
                return ValidationResult.Error("--until must be an ISO 8601 timestamp.");

            if (Statistics is not null && !StatisticsPeriods.Contains(Statistics))
                return ValidationResult.Error($"--statistics must be one of: {string.Join(", ", StatisticsPeriods)}.");

            return ValidationResult.Success();
        }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var since = DurationParser.Parse(settings.Since);
        var startTime = DateTimeOffset.UtcNow - since;
        var endTime = settings.Until is not null
            ? DateTimeOffset.Parse(settings.Until, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            : DateTimeOffset.UtcNow;

        return settings.Statistics is not null
            ? await RunStatisticsAsync(settings, startTime, endTime, cancellationToken)
            : await RunStateHistoryAsync(settings, startTime, cancellationToken);
    }

    private async Task<int> RunStateHistoryAsync(Settings settings, DateTimeOffset startTime, CancellationToken cancellationToken)
    {
        var path = BuildPath(startTime, settings.EntityId, settings.Until, settings.WithAttributes);
        var groups = await api.GetAsync<List<List<HistoryState>>>(path, cancellationToken);
        var states = groups.Count > 0 ? groups[0] : [];

        OutputHelper.WriteResult(settings, states,
            humanOutput: () => WriteHistoryHuman(states, settings.EntityId),
            porcelainOutput: () => WriteHistoryPorcelain(states));

        return 0;
    }

    private async Task<int> RunStatisticsAsync(Settings settings, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken)
    {
        var byEntity = await ws.GetStatisticsDuringPeriodAsync(startTime, endTime, [settings.EntityId], settings.Statistics!, cancellationToken);
        var rows = byEntity.TryGetValue(settings.EntityId, out var r) ? r.ToList() : [];

        OutputHelper.WriteResult(settings, rows,
            humanOutput: () => WriteStatisticsHuman(rows, settings.EntityId, settings.Statistics!),
            porcelainOutput: () => WriteStatisticsPorcelain(rows));

        return 0;
    }

    private static string BuildPath(DateTimeOffset start, string entity, string? until, bool withAttributes)
    {
        var basePath = $"/api/history/period/{Uri.EscapeDataString(start.ToString("o", CultureInfo.InvariantCulture))}";
        var queryParts = new List<string> { $"filter_entity_id={Uri.EscapeDataString(entity)}" };
        if (until is not null) queryParts.Add($"end_time={Uri.EscapeDataString(until)}");
        if (!withAttributes)
        {
            queryParts.Add("minimal_response");
            queryParts.Add("no_attributes");
        }
        return $"{basePath}?{string.Join('&', queryParts)}";
    }

    private static void WriteHistoryHuman(List<HistoryState> states, string entityId)
    {
        if (states.Count == 0)
        {
            AnsiConsole.MarkupLine($"[dim]No history for {entityId.EscapeMarkup()}.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Time")
            .AddColumn("State");

        foreach (var state in states)
        {
            table.AddRow(
                FormatTimeLocal(state.LastChanged ?? state.LastUpdated).EscapeMarkup(),
                (state.State ?? "").EscapeMarkup());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{states.Count} state change{(states.Count == 1 ? "" : "s")} for {entityId.EscapeMarkup()}[/]");
    }

    private static void WriteHistoryPorcelain(List<HistoryState> states)
    {
        OutputHelper.WriteColumns(
            ["WHEN", "STATE"],
            states.Select(s => new[]
            {
                FormatTimeIso(s.LastChanged ?? s.LastUpdated),
                s.State ?? ""
            }));
    }

    private static void WriteStatisticsHuman(List<StatisticsRow> rows, string entityId, string period)
    {
        if (rows.Count == 0)
        {
            AnsiConsole.MarkupLine($"[dim]No statistics recorded for {entityId.EscapeMarkup()}. (Sensor needs state_class: measurement to be aggregated.)[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Start")
            .AddColumn(new TableColumn("Mean").RightAligned())
            .AddColumn(new TableColumn("Min").RightAligned())
            .AddColumn(new TableColumn("Max").RightAligned())
            .AddColumn(new TableColumn("Sum").RightAligned());

        foreach (var row in rows)
        {
            table.AddRow(
                FormatStart(row.Start).EscapeMarkup(),
                FormatNumber(row.Mean),
                FormatNumber(row.Min),
                FormatNumber(row.Max),
                FormatNumber(row.Sum));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{rows.Count} {period} period{(rows.Count == 1 ? "" : "s")} for {entityId.EscapeMarkup()}[/]");
    }

    private static void WriteStatisticsPorcelain(List<StatisticsRow> rows)
    {
        OutputHelper.WriteColumns(
            ["START", "MEAN", "MIN", "MAX", "SUM"],
            rows.Select(r => new[]
            {
                FormatStartIso(r.Start),
                FormatNumber(r.Mean),
                FormatNumber(r.Min),
                FormatNumber(r.Max),
                FormatNumber(r.Sum)
            }));
    }

    private static string FormatNumber(double? value) =>
        value is null ? "" : value.Value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatStart(JsonElement start) => start.ValueKind switch
    {
        JsonValueKind.Number => DateTimeOffset.FromUnixTimeMilliseconds(start.GetInt64())
            .ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        JsonValueKind.String => FormatTimeLocal(start.GetString()),
        _ => start.ToString()
    };

    private static string FormatStartIso(JsonElement start) => start.ValueKind switch
    {
        JsonValueKind.Number => DateTimeOffset.FromUnixTimeMilliseconds(start.GetInt64())
            .ToString("o", CultureInfo.InvariantCulture),
        JsonValueKind.String => FormatTimeIso(start.GetString()),
        _ => start.ToString()
    };

    private static string FormatTimeLocal(string? iso) =>
        DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : iso ?? "";

    private static string FormatTimeIso(string? iso) =>
        DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToString("o", CultureInfo.InvariantCulture)
            : iso ?? "";
}

