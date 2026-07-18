using System.ComponentModel;
using System.Text.Json;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationReconfigureCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationReconfigureCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID to reconfigure (from `haus integration list`)")]
        public required string EntryId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("New settings JSON to submit; omit to inspect what the flow is asking for")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read settings JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Data);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        // Reconfigure is user-startable, but needs the integration domain (handler) to begin.
        var entries = await client.Integration.ListAsync(cancellationToken);
        var entry = entries.FirstOrDefault(e => e.EntryId == settings.EntryId);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"Config entry not found: {settings.EntryId}");
            return 1;
        }

        var step = await client.Integration.StartReconfigureAsync(entry.Domain, settings.EntryId, cancellationToken);
        if (step.Type == "abort")
        {
            OutputHelper.WriteError(settings, $"Reconfigure not available: {step.Reason ?? "unknown reason"}");
            return 1;
        }

        var raw = TextInput.Resolve(settings.Data, settings.FromFile);
        if (raw is null)
        {
            OutputHelper.WriteResult(settings, step,
                () =>
                {
                    AnsiConsole.MarkupLine(
                        $"[bold]Reconfigure[/] [bold]{entry.Domain.EscapeMarkup()}[/] [dim](step: {step.StepId ?? "?"})[/]");
                    ConfigFlow.WriteInspectBody(step,
                        $"Submit with: haus integration reconfigure {settings.EntryId} --data '{{...}}'");
                },
                () => ConfigFlow.WritePorcelainInspect(step));

            // We started this flow just to look — don't leave it dangling in HA.
            try { await client.Integration.AbortFlowAsync(step.FlowId, cancellationToken); }
            catch { /* best-effort cleanup */ }
            return 0;
        }

        var userInput = JsonSerializer.Deserialize<JsonElement>(raw);
        var result = await client.Integration.SubmitFlowAsync(step.FlowId, userInput, cancellationToken);
        return ConfigFlow.WriteResult(settings, result, settings.EntryId, "Reconfigured");
    }
}
