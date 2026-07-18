using System.ComponentModel;
using System.Text.Json;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Backup;

public sealed class BackupGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<BackupGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<backup_id>")]
        [Description("Backup ID (from `haus backup list`)")]
        public required string BackupId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var doc = await client.Backup.GetAsync(settings.BackupId, cancellationToken);

        OutputHelper.WriteResult(settings, doc,
            () => WriteHumanOutput(doc),
            () => WritePorcelainOutput(doc));

        return 0;
    }

    private static void WriteHumanOutput(JsonElement doc)
    {
        var table = new Table().Border(TableBorder.Rounded).AddColumn("Property").AddColumn("Value");
        void Row(string label, string key)
        {
            if (doc.TryGetProperty(key, out var v) && v.ValueKind is not JsonValueKind.Null)
                table.AddRow($"[dim]{label}[/]", v.ToString().EscapeMarkup());
        }

        Row("Backup ID", "backup_id");
        Row("Name", "name");
        Row("Date", "date");
        Row("HA Version", "homeassistant_version");
        Row("HA Included", "homeassistant_included");
        Row("Database Included", "database_included");
        if (doc.TryGetProperty("addons", out var a) && a.ValueKind == JsonValueKind.Array)
            table.AddRow("[dim]Add-ons[/]", a.GetArrayLength().ToString());
        if (doc.TryGetProperty("folders", out var f) && f.ValueKind == JsonValueKind.Array)
            table.AddRow("[dim]Folders[/]", string.Join(", ", f.EnumerateArray().Select(x => x.GetString())).EscapeMarkup());
        if (doc.TryGetProperty("agents", out var ag) && ag.ValueKind == JsonValueKind.Object)
            table.AddRow("[dim]Agents[/]", string.Join(", ", ag.EnumerateObject().Select(p => p.Name)).EscapeMarkup());

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Use --json for the full manifest.[/]");
    }

    private static void WritePorcelainOutput(JsonElement doc)
    {
        void Kv(string key)
        {
            if (doc.TryGetProperty(key, out var v) && v.ValueKind is not JsonValueKind.Null)
                OutputHelper.WriteKeyValue(key, v.ToString());
        }

        foreach (var key in new[] { "backup_id", "name", "date", "homeassistant_version", "homeassistant_included", "database_included" })
            Kv(key);
    }
}
