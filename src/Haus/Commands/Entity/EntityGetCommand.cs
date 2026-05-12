using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed class EntityGetCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<EntityGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID (e.g. light.kitchen)")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = await ws.SendCommandAsync(new
        {
            type = EntityRegistryCommands.Get,
            entity_id = settings.EntityId
        }, cancellationToken);

        var entry = result.Deserialize<EntityRegistryEntry>();
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"Entity '{settings.EntityId}' not found in registry.");
            return 1;
        }

        OutputHelper.WriteResult(settings, entry,
            () => WriteHumanOutput(entry),
            () => WritePorcelainOutput(entry));

        return 0;
    }

    private static void WriteHumanOutput(EntityRegistryEntry entry)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("[bold]Entity ID[/]", entry.EntityId.EscapeMarkup());
        table.AddRow("[bold]Name[/]", entry.DisplayName.EscapeMarkup());
        if (entry.Name is not null && entry.OriginalName is not null && entry.Name != entry.OriginalName)
            table.AddRow("[dim]Original Name[/]", entry.OriginalName.EscapeMarkup());
        table.AddRow("[bold]Platform[/]", (entry.Platform ?? "").EscapeMarkup());
        table.AddRow("[bold]Status[/]", entry.Status switch
        {
            "disabled" => "[red]disabled[/]",
            "hidden" => "[yellow]hidden[/]",
            _ => "[green]active[/]"
        });
        if (entry.AreaId is not null) table.AddRow("[dim]Area[/]", entry.AreaId.EscapeMarkup());
        if (entry.DeviceId is not null) table.AddRow("[dim]Device[/]", entry.DeviceId.EscapeMarkup());
        if (entry.Icon is not null) table.AddRow("[dim]Icon[/]", entry.Icon.EscapeMarkup());
        if (entry.EntityCategory is not null) table.AddRow("[dim]Category[/]", entry.EntityCategory.EscapeMarkup());
        if (entry.DeviceClass is not null) table.AddRow("[dim]Device Class[/]", entry.DeviceClass.EscapeMarkup());
        if (entry.UniqueId is not null) table.AddRow("[dim]Unique ID[/]", entry.UniqueId.EscapeMarkup());
        if (entry.Labels is { Count: > 0 }) table.AddRow("[dim]Labels[/]", string.Join(", ", entry.Labels).EscapeMarkup());

        AnsiConsole.Write(table);
    }

    private static void WritePorcelainOutput(EntityRegistryEntry entry)
    {
        OutputHelper.WriteKeyValue("entity_id", entry.EntityId);
        OutputHelper.WriteKeyValue("name", entry.DisplayName);
        OutputHelper.WriteKeyValue("original_name", entry.OriginalName ?? "");
        OutputHelper.WriteKeyValue("platform", entry.Platform ?? "");
        OutputHelper.WriteKeyValue("status", entry.Status);
        OutputHelper.WriteKeyValue("area_id", entry.AreaId ?? "");
        OutputHelper.WriteKeyValue("device_id", entry.DeviceId ?? "");
        OutputHelper.WriteKeyValue("icon", entry.Icon ?? "");
        OutputHelper.WriteKeyValue("entity_category", entry.EntityCategory ?? "");
        OutputHelper.WriteKeyValue("device_class", entry.DeviceClass ?? "");
        OutputHelper.WriteKeyValue("unique_id", entry.UniqueId ?? "");
        OutputHelper.WriteKeyValue("labels", entry.Labels is null ? "" : string.Join(",", entry.Labels));
    }
}
