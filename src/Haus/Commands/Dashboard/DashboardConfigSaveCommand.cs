using System.ComponentModel;
using Haus.HassClient;
using System.Text.Json;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Dashboard;

public sealed class DashboardConfigSaveCommand(IAuthService auth, IHassClient client)
    : HausCommand<DashboardConfigSaveCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<url_path>")]
        [Description("Dashboard URL path ('lovelace' for the default)")]
        public required string UrlPath { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Full dashboard config as JSON (must contain a 'views' array)")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read config JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var json = TextInput.Resolve(settings.Data, settings.FromFile)!;
        JsonElement config;
        try
        {
            using var doc = JsonDocument.Parse(json);
            config = doc.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            OutputHelper.WriteError(settings, $"Invalid JSON: {ex.Message}");
            return 1;
        }
        if (config.ValueKind != JsonValueKind.Object || !config.TryGetProperty("views", out _))
        {
            OutputHelper.WriteError(settings, "Config must be a JSON object containing a 'views' array.");
            return 1;
        }

        var dashboards = await client.Lovelace.ListDashboardsAsync(cancellationToken);
        var entry = dashboards.FirstOrDefault(d => d.UrlPath == settings.UrlPath);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"No dashboard with url_path '{settings.UrlPath}'.");
            return 1;
        }
        if (!string.Equals(entry.Mode, "storage", StringComparison.Ordinal))
        {
            OutputHelper.WriteError(settings, $"Dashboard '{settings.UrlPath}' is in '{entry.Mode}' mode; only storage-mode dashboards are editable.");
            return 1;
        }

        var configUrlPath = string.Equals(settings.UrlPath, "lovelace", StringComparison.Ordinal) ? null : settings.UrlPath;
        await client.Lovelace.SaveConfigAsync(configUrlPath, config, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "saved", url_path = settings.UrlPath },
            () => AnsiConsole.MarkupLine($"[green]Saved[/] config for [bold]{settings.UrlPath.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.UrlPath));

        return 0;
    }
}
