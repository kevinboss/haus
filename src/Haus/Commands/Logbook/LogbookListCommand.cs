using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;
using JetBrains.Annotations;

namespace Haus.Commands.Logbook;

public sealed class LogbookListCommand(IAuthService auth, IHassApiClient api) : HausCommand<LogbookListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("-e|--entity <ENTITY_ID>")]
        [Description("Filter to a single entity")]
        public string? EntityId { get; init; }

        [CommandOption("-s|--since <DURATION>")]
        [Description("How far back to look (e.g. 30m, 1h, 2d). Default: 1h.")]
        public string Since { get; init; } = "1h";

        [CommandOption("-u|--until <ISO_TIMESTAMP>")]
        [Description("End timestamp (ISO 8601). Default: now.")]
        public string? Until { get; init; }

        public override ValidationResult Validate()
        {
            try { _ = DurationParser.Parse(Since); }
            catch (FormatException ex) { return ValidationResult.Error(ex.Message); }

            if (Until is not null && !DateTimeOffset.TryParse(Until, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
                return ValidationResult.Error("--until must be an ISO 8601 timestamp.");

            return ValidationResult.Success();
        }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var since = DurationParser.Parse(settings.Since);
        var startTime = DateTimeOffset.UtcNow - since;
        var path = BuildPath(startTime, settings.EntityId, settings.Until);

        var entries = await api.GetAsync<List<LogbookEntry>>(path, cancellationToken);

        OutputHelper.WriteResult(settings, entries,
            humanOutput: () => WriteHumanOutput(entries),
            porcelainOutput: () => WritePorcelainOutput(entries));

        return 0;
    }

    private static string BuildPath(DateTimeOffset start, string? entity, string? until)
    {
        var basePath = $"/api/logbook/{Uri.EscapeDataString(start.ToString("o", CultureInfo.InvariantCulture))}";
        var queryParts = new List<string>();
        if (entity is not null) queryParts.Add($"entity={Uri.EscapeDataString(entity)}");
        if (until is not null) queryParts.Add($"end_time={Uri.EscapeDataString(until)}");
        return queryParts.Count == 0 ? basePath : $"{basePath}?{string.Join('&', queryParts)}";
    }

    private static void WriteHumanOutput(List<LogbookEntry> entries)
    {
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No logbook entries.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Time")
            .AddColumn("Name")
            .AddColumn("Message")
            .AddColumn("Entity");

        foreach (var entry in entries)
        {
            table.AddRow(
                FormatTimeLocal(entry.When).EscapeMarkup(),
                (entry.Name ?? "").EscapeMarkup(),
                (entry.Message ?? entry.State ?? "").EscapeMarkup(),
                (entry.EntityId ?? "").EscapeMarkup());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{entries.Count} entr{(entries.Count == 1 ? "y" : "ies")}[/]");
    }

    private static void WritePorcelainOutput(List<LogbookEntry> entries)
    {
        OutputHelper.WriteColumns(
            ["WHEN", "ENTITY ID", "NAME", "DOMAIN", "MESSAGE"],
            entries.Select(e => new[]
            {
                FormatTimeIso(e.When),
                e.EntityId ?? "",
                e.Name ?? "",
                e.Domain ?? "",
                e.Message ?? e.State ?? ""
            }));
    }

    private static string FormatTimeLocal(JsonElement when) =>
        TryParseTimestamp(when, out var dt)
            ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : when.ToString();

    private static string FormatTimeIso(JsonElement when) =>
        TryParseTimestamp(when, out var dt)
            ? dt.ToString("o", CultureInfo.InvariantCulture)
            : when.ToString();

    private static bool TryParseTimestamp(JsonElement when, out DateTimeOffset result)
    {
        if (when.ValueKind == JsonValueKind.Number)
        {
            result = DateTimeOffset.FromUnixTimeMilliseconds((long)(when.GetDouble() * 1000));
            return true;
        }
        if (when.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(when.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
            return true;
        result = default;
        return false;
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record LogbookEntry(
    [property: JsonPropertyName("when")] JsonElement When,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("domain")] string? Domain,
    [property: JsonPropertyName("entity_id")] string? EntityId,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("context_user_id")] string? ContextUserId);
