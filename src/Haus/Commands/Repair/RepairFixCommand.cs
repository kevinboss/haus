using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Repair;

public sealed class RepairFixCommand(IAuthService auth, IHassClient client)
    : HausCommand<RepairFixCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<domain>")]
        [Description("Integration domain that raised the issue (from `haus repair list`)")]
        public required string Domain { get; init; }

        [CommandArgument(1, "<issue_id>")]
        [Description("Issue ID (from `haus repair list`)")]
        public required string IssueId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("JSON to submit to the fix flow. Omit to inspect a flow that wants input — but note a confirm-only repair has nothing to inspect and is applied immediately")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read the fix flow JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() => JsonInput.ValidateOptional(Data, FromFile);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var issue = await client.Repairs.GetAsync(settings.Domain, settings.IssueId, cancellationToken);
        if (issue is null)
        {
            OutputHelper.WriteError(settings,
                $"No repair issue '{settings.IssueId}' for domain '{settings.Domain}'. " +
                "Run `haus repair list` to see the current issues.");
            return 1;
        }

        if (!issue.IsFixable)
        {
            OutputHelper.WriteError(settings,
                $"'{settings.Domain}/{settings.IssueId}' has no fix flow — it has to be resolved at the source " +
                "(usually by editing your YAML or the integration's settings). " +
                $"See `haus repair get {settings.Domain} {settings.IssueId}`, or dismiss it with " +
                $"`haus repair ignore {settings.Domain} {settings.IssueId}`.");
            return 1;
        }

        var label = $"{settings.Domain}/{settings.IssueId}";
        var step = await client.Repairs.StartFixAsync(settings.Domain, settings.IssueId, cancellationToken);

        if (step.Type is "create_entry" or "abort")
            return Finish(settings, step, label);

        var raw = TextInput.Resolve(settings.Data, settings.FromFile);

        if (raw is null && !ConfigFlow.IsConfirmation(step))
        {
            await client.Repairs.AbortFixAsync(step.FlowId, cancellationToken);

            OutputHelper.WriteResult(settings, step,
                () =>
                {
                    AnsiConsole.MarkupLine(
                        $"[bold]Fix pending[/] for [bold]{label.EscapeMarkup()}[/] [dim](step: {step.StepId ?? "?"})[/]");
                    ConfigFlow.WriteInspectBody(step,
                        $"Submit with: haus repair fix {settings.Domain} {settings.IssueId} --data '{{...}}'");
                    AnsiConsole.MarkupLine("[dim]Nothing was changed — the flow was closed again.[/]");
                },
                () => ConfigFlow.WritePorcelainInspect(step));
            return 0;
        }

        object userInput = raw is null
            ? new { }
            : JsonSerializer.Deserialize<JsonElement>(raw);

        var result = await client.Repairs.SubmitFixAsync(step.FlowId, userInput, cancellationToken);
        return Finish(settings, result, label);
    }

    private static int Finish(Settings settings, OptionsFlowStep result, string label)
    {
        if (result.Type != "abort" || ConfigFlow.IsSuccess(result))
            return ConfigFlow.WriteResult(settings, result, label, "Fixed");

        var reason = result.Reason ?? "unknown reason";
        OutputHelper.WriteResult(settings, result,
            () =>
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]Flow ended[/] for [bold]{label.EscapeMarkup()}[/]: {reason.EscapeMarkup()}");
                AnsiConsole.MarkupLine("[dim]No fix was applied. Run `haus repair list` to see whether the issue is still open.[/]");
            },
            () => OutputHelper.WriteKeyValue("aborted", reason));
        return 0;
    }
}
