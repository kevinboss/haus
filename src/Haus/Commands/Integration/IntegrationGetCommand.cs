using System.ComponentModel;
using System.Text.Json;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID (from `haus integration list`)")]
        public required string EntryId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entries = await client.Integration.ListAsync(cancellationToken);
        var entry = entries.FirstOrDefault(e =>
            string.Equals(e.EntryId, settings.EntryId, StringComparison.Ordinal));
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"Config entry not found: {settings.EntryId}");
            return 1;
        }

        OptionsFlowStep? schema = null;
        if (entry.SupportsOptions)
        {
            try
            {
                schema = await client.Integration.InitOptionsAsync(settings.EntryId, cancellationToken);
                if (schema.FlowId is { Length: > 0 } flowId)
                {
                    try { await client.Integration.AbortOptionsAsync(flowId, cancellationToken); }
                    catch { /* best-effort cleanup */ }
                }
            }
            catch
            {
                // Integration claims to support options but the flow failed — show entry only.
            }
        }

        OutputHelper.WriteResult(settings, new { entry, schema },
            humanOutput: () => WriteHuman(entry, schema),
            porcelainOutput: () => WritePorcelain(entry, schema));

        return 0;
    }

    private static void WriteHuman(ConfigEntry e, OptionsFlowStep? schema)
    {
        AnsiConsole.MarkupLine($"[bold]{e.Title.EscapeMarkup()}[/] [dim]({e.Domain.EscapeMarkup()})[/]");
        AnsiConsole.WriteLine();

        var meta = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn("Key").AddColumn("Value");
        meta.AddRow("[dim]Entry ID[/]", e.EntryId.EscapeMarkup());
        meta.AddRow("[dim]State[/]", e.State?.EscapeMarkup() ?? "");
        meta.AddRow("[dim]Source[/]", e.Source?.EscapeMarkup() ?? "");
        meta.AddRow("[dim]Supports options[/]", e.SupportsOptions ? "yes" : "no");
        meta.AddRow("[dim]Disabled[/]", e.DisabledBy is null ? "no" : $"yes ({e.DisabledBy.EscapeMarkup()})");
        if (e.Reason is not null) meta.AddRow("[dim]Reason[/]", e.Reason.EscapeMarkup());
        AnsiConsole.Write(meta);

        if (schema is null || schema.Type != "form" || schema.DataSchema is not { ValueKind: JsonValueKind.Array } fields)
        {
            if (e.SupportsOptions)
                AnsiConsole.MarkupLine("\n[dim]No options schema returned.[/]");
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Options[/] [dim](step: {schema.StepId?.EscapeMarkup() ?? "init"})[/]");
        var table = new Table().Border(TableBorder.Rounded)
            .AddColumn("Field").AddColumn("Type").AddColumn("Default").AddColumn("Required");

        foreach (var f in fields.EnumerateArray())
        {
            var name = f.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
            var type = f.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
            var def = f.TryGetProperty("default", out var d) ? d.ToString() : "";
            var required = f.TryGetProperty("required", out var r) && r.GetBoolean();
            table.AddRow(
                name.EscapeMarkup(),
                type.EscapeMarkup(),
                def.EscapeMarkup(),
                required ? "[yellow]yes[/]" : "[dim]no[/]");
        }
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]Submit with: haus integration configure {e.EntryId} --data '{{...}}'[/]");
    }

    private static void WritePorcelain(ConfigEntry e, OptionsFlowStep? schema)
    {
        OutputHelper.WriteKeyValue("entry_id", e.EntryId);
        OutputHelper.WriteKeyValue("domain", e.Domain);
        OutputHelper.WriteKeyValue("title", e.Title);
        OutputHelper.WriteKeyValue("state", e.State ?? "");
        OutputHelper.WriteKeyValue("source", e.Source ?? "");
        OutputHelper.WriteKeyValue("supports_options", e.SupportsOptions ? "yes" : "no");
        OutputHelper.WriteKeyValue("disabled_by", e.DisabledBy ?? "");

        if (schema is null || schema.Type != "form" || schema.DataSchema is not { ValueKind: JsonValueKind.Array } fields)
            return;

        Console.WriteLine();
        OutputHelper.WriteColumns(
            ["FIELD", "TYPE", "DEFAULT", "REQUIRED"],
            fields.EnumerateArray().Select(f =>
            {
                var name = f.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var type = f.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
                var def = f.TryGetProperty("default", out var d) ? d.ToString() : "";
                var required = f.TryGetProperty("required", out var r) && r.GetBoolean();
                return new[] { name, type, def, required ? "yes" : "no" };
            }));
    }
}
