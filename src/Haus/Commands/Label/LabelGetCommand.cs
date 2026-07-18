using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Label;

public sealed class LabelGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<label_id>")]
        [Description("Label ID (e.g. critical)")]
        public required string LabelId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entry = await client.Label.GetAsync(settings.LabelId, cancellationToken);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"Label '{settings.LabelId}' not found in registry.");
            return 1;
        }

        OutputHelper.WriteResult(settings, entry,
            () => WriteHumanOutput(entry),
            () => WritePorcelainOutput(entry));

        return 0;
    }

    private static void WriteHumanOutput(LabelEntry entry)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("[bold]Label ID[/]", entry.LabelId.EscapeMarkup());
        table.AddRow("[bold]Name[/]", entry.Name.EscapeMarkup());
        if (entry.Color is not null) table.AddRow("[dim]Color[/]", entry.Color.EscapeMarkup());
        if (entry.Icon is not null) table.AddRow("[dim]Icon[/]", entry.Icon.EscapeMarkup());
        if (entry.Description is not null) table.AddRow("[dim]Description[/]", entry.Description.EscapeMarkup());

        AnsiConsole.Write(table);
    }

    private static void WritePorcelainOutput(LabelEntry entry)
    {
        OutputHelper.WriteKeyValue("label_id", entry.LabelId);
        OutputHelper.WriteKeyValue("name", entry.Name);
        OutputHelper.WriteKeyValue("color", entry.Color ?? "");
        OutputHelper.WriteKeyValue("icon", entry.Icon ?? "");
        OutputHelper.WriteKeyValue("description", entry.Description ?? "");
    }
}
