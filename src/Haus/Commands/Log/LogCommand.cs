using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Log;

public sealed class LogCommand(IAuthService auth, IHassWebSocketClient ws) : HausCommand<LogCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("-n|--limit <COUNT>")]
        [Description("Show only the most recent N entries")]
        public int? Limit { get; init; }

        [CommandOption("-l|--level <LEVEL>")]
        [Description("Filter by level (error, warning, info, debug)")]
        public string? Level { get; init; }

        [CommandOption("--with-trace")]
        [Description("Include exception stack traces")]
        public bool WithTrace { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = await ws.SendCommandAsync(new { type = "system_log/list" }, cancellationToken);

        var entries = result.EnumerateArray().Select(SystemLogEntry.From).ToList();
        entries.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

        if (settings.Level is { } level)
            entries = entries.Where(e => string.Equals(e.Level, level, StringComparison.OrdinalIgnoreCase)).ToList();

        if (settings.Limit is { } n && n > 0)
            entries = entries.Take(n).ToList();

        OutputHelper.WriteResult(settings, entries,
            humanOutput: () => WriteHumanOutput(entries, settings.WithTrace),
            porcelainOutput: () => WritePorcelainOutput(entries));

        return 0;
    }

    private static void WriteHumanOutput(List<SystemLogEntry> entries, bool withTrace)
    {
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No log entries.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Time")
            .AddColumn("Level")
            .AddColumn("Logger")
            .AddColumn("Message");

        foreach (var entry in entries)
        {
            var msg = entry.Message.EscapeMarkup();
            if (entry.Count > 1) msg = $"{msg} [dim](×{entry.Count})[/]";
            if (withTrace && !string.IsNullOrWhiteSpace(entry.Exception))
                msg += $"\n[dim]{entry.Exception.EscapeMarkup()}[/]";

            table.AddRow(
                FormatTimeLocal(entry.Timestamp).EscapeMarkup(),
                LevelMarkup(entry.Level),
                entry.Name.EscapeMarkup(),
                msg);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{entries.Count} entr{(entries.Count == 1 ? "y" : "ies")}[/]");
    }

    private static void WritePorcelainOutput(List<SystemLogEntry> entries)
    {
        OutputHelper.WriteColumns(
            ["WHEN", "LEVEL", "LOGGER", "COUNT", "MESSAGE"],
            entries.Select(e => new[]
            {
                FormatTimeIso(e.Timestamp),
                e.Level,
                e.Name,
                e.Count.ToString(CultureInfo.InvariantCulture),
                e.Message.ReplaceLineEndings(" ")
            }));
    }

    private static string LevelMarkup(string level) => level.ToUpperInvariant() switch
    {
        "ERROR" or "CRITICAL" or "FATAL" => $"[red]{level.EscapeMarkup()}[/]",
        "WARNING" or "WARN" => $"[yellow]{level.EscapeMarkup()}[/]",
        "INFO" => $"[cyan]{level.EscapeMarkup()}[/]",
        _ => level.EscapeMarkup()
    };

    private static string FormatTimeLocal(double unix) =>
        DateTimeOffset.FromUnixTimeMilliseconds((long)(unix * 1000))
            .ToLocalTime()
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    private static string FormatTimeIso(double unix) =>
        DateTimeOffset.FromUnixTimeMilliseconds((long)(unix * 1000))
            .ToString("o", CultureInfo.InvariantCulture);
}

internal sealed record SystemLogEntry(
    double Timestamp,
    string Level,
    string Name,
    string Message,
    string? Exception,
    int Count)
{
    public static SystemLogEntry From(JsonElement el)
    {
        var msgEl = el.GetProperty("message");
        var message = msgEl.ValueKind == JsonValueKind.Array
            ? string.Join(" ", msgEl.EnumerateArray().Select(m => m.GetString() ?? ""))
            : msgEl.GetString() ?? "";

        return new SystemLogEntry(
            Timestamp: el.GetProperty("timestamp").GetDouble(),
            Level: el.TryGetProperty("level", out var lv) ? lv.GetString() ?? "" : "",
            Name: el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Message: message,
            Exception: el.TryGetProperty("exception", out var ex) ? ex.GetString() : null,
            Count: el.TryGetProperty("count", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : 1);
    }
}
