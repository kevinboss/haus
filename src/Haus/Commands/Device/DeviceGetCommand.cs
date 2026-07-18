using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Device;

public sealed class DeviceGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<DeviceGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<device_id>")]
        [Description("Device ID (from `haus device list`)")]
        public required string DeviceId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entry = await client.Device.GetAsync(settings.DeviceId, cancellationToken);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"Device '{settings.DeviceId}' not found in registry.");
            return 1;
        }

        OutputHelper.WriteResult(settings, entry,
            () => WriteHumanOutput(entry),
            () => WritePorcelainOutput(entry));

        return 0;
    }

    private static void WriteHumanOutput(DeviceRegistryEntry entry)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("[bold]Device ID[/]", entry.Id.EscapeMarkup());
        table.AddRow("[bold]Name[/]", entry.DisplayName.EscapeMarkup());
        if (entry.NameByUser is not null && entry.Name is not null && entry.NameByUser != entry.Name)
            table.AddRow("[dim]Original Name[/]", entry.Name.EscapeMarkup());
        table.AddRow("[bold]Status[/]", entry.Status == "disabled" ? "[red]disabled[/]" : "[green]active[/]");
        if (entry.Manufacturer is not null) table.AddRow("[dim]Manufacturer[/]", entry.Manufacturer.EscapeMarkup());
        if (entry.Model is not null) table.AddRow("[dim]Model[/]", entry.Model.EscapeMarkup());
        if (entry.ModelId is not null) table.AddRow("[dim]Model ID[/]", entry.ModelId.EscapeMarkup());
        if (entry.SwVersion is not null) table.AddRow("[dim]SW Version[/]", entry.SwVersion.EscapeMarkup());
        if (entry.HwVersion is not null) table.AddRow("[dim]HW Version[/]", entry.HwVersion.EscapeMarkup());
        if (entry.AreaId is not null) table.AddRow("[dim]Area[/]", entry.AreaId.EscapeMarkup());
        if (entry.EntryType is not null) table.AddRow("[dim]Entry Type[/]", entry.EntryType.EscapeMarkup());
        if (entry.ConfigEntries is { Count: > 0 }) table.AddRow("[dim]Config Entries[/]", string.Join(", ", entry.ConfigEntries).EscapeMarkup());
        if (entry.ConfigurationUrl is not null) table.AddRow("[dim]Config URL[/]", entry.ConfigurationUrl.EscapeMarkup());
        if (entry.Labels is { Count: > 0 }) table.AddRow("[dim]Labels[/]", string.Join(", ", entry.Labels).EscapeMarkup());

        AnsiConsole.Write(table);
    }

    private static void WritePorcelainOutput(DeviceRegistryEntry entry)
    {
        OutputHelper.WriteKeyValue("device_id", entry.Id);
        OutputHelper.WriteKeyValue("name", entry.DisplayName);
        OutputHelper.WriteKeyValue("original_name", entry.Name ?? "");
        OutputHelper.WriteKeyValue("status", entry.Status);
        OutputHelper.WriteKeyValue("manufacturer", entry.Manufacturer ?? "");
        OutputHelper.WriteKeyValue("model", entry.Model ?? "");
        OutputHelper.WriteKeyValue("model_id", entry.ModelId ?? "");
        OutputHelper.WriteKeyValue("sw_version", entry.SwVersion ?? "");
        OutputHelper.WriteKeyValue("hw_version", entry.HwVersion ?? "");
        OutputHelper.WriteKeyValue("area_id", entry.AreaId ?? "");
        OutputHelper.WriteKeyValue("entry_type", entry.EntryType ?? "");
        OutputHelper.WriteKeyValue("config_entries", entry.ConfigEntries is null ? "" : string.Join(",", entry.ConfigEntries));
        OutputHelper.WriteKeyValue("configuration_url", entry.ConfigurationUrl ?? "");
        OutputHelper.WriteKeyValue("labels", entry.Labels is null ? "" : string.Join(",", entry.Labels));
    }
}
