using System.ComponentModel;
using System.Text.Json;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationReauthCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationReauthCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID with a pending reauth (from `haus integration list`)")]
        public required string EntryId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Credentials JSON to submit; omit to inspect what the flow is asking for")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read credentials JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Data);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        // Reauth flows are system-initiated: HA creates one when an integration's credentials fail.
        // We can only find and complete it — not start it.
        var flows = await client.Integration.ListInProgressFlowsAsync(cancellationToken);
        var flow = flows.FirstOrDefault(f =>
            f.Context?.EntryId == settings.EntryId && f.Context?.Source == "reauth");

        if (flow is null)
        {
            OutputHelper.WriteError(settings,
                $"No reauth flow is pending for '{settings.EntryId}'. HA starts reauth automatically when an " +
                "integration's credentials fail — there's nothing to re-authenticate right now. To change " +
                "credentials proactively, use `haus integration reconfigure`.");
            return 1;
        }

        var step = await client.Integration.GetFlowAsync(flow.FlowId, cancellationToken);
        var raw = TextInput.Resolve(settings.Data, settings.FromFile);

        if (raw is null)
        {
            OutputHelper.WriteResult(settings, step,
                () =>
                {
                    AnsiConsole.MarkupLine(
                        $"[bold]Reauth pending[/] for [bold]{settings.EntryId.EscapeMarkup()}[/] [dim](step: {step.StepId ?? "?"})[/]");
                    ConfigFlow.WriteInspectBody(step,
                        $"Submit with: haus integration reauth {settings.EntryId} --data '{{...}}'");
                },
                () => ConfigFlow.WritePorcelainInspect(step));
            return 0;
        }

        var userInput = JsonSerializer.Deserialize<JsonElement>(raw);
        var result = await client.Integration.SubmitFlowAsync(flow.FlowId, userInput, cancellationToken);
        return ConfigFlow.WriteResult(settings, result, settings.EntryId, "Re-authenticated");
    }
}
