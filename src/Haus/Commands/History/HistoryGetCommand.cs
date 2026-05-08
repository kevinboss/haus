using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.History;

public sealed class HistoryGetCommand(IAuthService auth, IHassApiClient api) : HausCommand<HistoryGetCommand.Settings>(auth)
{
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

        public override ValidationResult Validate()
        {
            try { _ = DurationParser.Parse(Since); }
            catch (FormatException ex) { return ValidationResult.Error(ex.Message); }

            if (Until is not null && !DateTimeOffset.TryParse(Until, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
                return ValidationResult.Error("--until must be an ISO 8601 timestamp.");

            return ValidationResult.Success();
        }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var since = DurationParser.Parse(settings.Since);
        var startTime = DateTimeOffset.UtcNow - since;
        var path = BuildPath(startTime, settings.EntityId, settings.Until, settings.WithAttributes);

        var groups = await api.GetAsync<List<List<HistoryState>>>(path, cancellationToken);
        var states = groups.Count > 0 ? groups[0] : [];

        OutputHelper.WriteResult(settings, states,
            humanOutput: () => WriteHumanOutput(states, settings.EntityId),
            porcelainOutput: () => WritePorcelainOutput(states));

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

    private static void WriteHumanOutput(List<HistoryState> states, string entityId)
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

    private static void WritePorcelainOutput(List<HistoryState> states)
    {
        OutputHelper.WriteColumns(
            ["WHEN", "STATE"],
            states.Select(s => new[]
            {
                FormatTimeIso(s.LastChanged ?? s.LastUpdated),
                s.State ?? ""
            }));
    }

    private static string FormatTimeLocal(string? iso) =>
        DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : iso ?? "";

    private static string FormatTimeIso(string? iso) =>
        DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToString("o", CultureInfo.InvariantCulture)
            : iso ?? "";
}

internal sealed record HistoryState(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("last_changed")] string? LastChanged,
    [property: JsonPropertyName("last_updated")] string? LastUpdated,
    [property: JsonPropertyName("entity_id")] string? EntityId);
