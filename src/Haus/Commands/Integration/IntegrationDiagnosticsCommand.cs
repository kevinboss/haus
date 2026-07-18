using System.ComponentModel;
using System.Text.Json;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationDiagnosticsCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationDiagnosticsCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID (from `haus integration list`)")]
        public required string EntryId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var diagnostics = await client.Integration.GetDiagnosticsAsync(settings.EntryId, cancellationToken);

        OutputHelper.WriteResult(settings, diagnostics,
            () => WriteHumanOutput(diagnostics),
            () => Console.WriteLine(diagnostics.GetRawText()));

        return 0;
    }

    private static void WriteHumanOutput(JsonElement doc)
    {
        if (doc.ValueKind != JsonValueKind.Object)
        {
            AnsiConsole.WriteLine(doc.GetRawText());
            return;
        }

        var table = new Table().Border(TableBorder.Rounded)
            .AddColumn("Key").AddColumn("Type").AddColumn(new TableColumn("Size").RightAligned());
        foreach (var prop in doc.EnumerateObject())
        {
            var (type, size) = Describe(prop.Value);
            table.AddRow(prop.Name.EscapeMarkup(), type, size);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Redacted diagnostics. Use --json for the full document (e.g. to attach to a bug report).[/]");
    }

    private static (string type, string size) Describe(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Object => ("object", $"{e.EnumerateObject().Count()} keys"),
        JsonValueKind.Array => ("array", $"{e.GetArrayLength()} items"),
        JsonValueKind.String => ("string", ""),
        JsonValueKind.Number => ("number", ""),
        JsonValueKind.True or JsonValueKind.False => ("bool", ""),
        _ => ("null", "")
    };
}
