using System.ComponentModel;
using System.Text.Json;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationConfigureCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationConfigureCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID (from `haus integration list`)")]
        public required string EntryId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Options JSON to submit")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read options JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var init = await client.Integration.InitOptionsAsync(settings.EntryId, cancellationToken);
        var raw = TextInput.Resolve(settings.Data, settings.FromFile)
            ?? throw new InvalidOperationException("Expected JSON input (validated upstream).");
        var userInput = JsonSerializer.Deserialize<JsonElement>(raw);
        var result = await client.Integration.ConfigureOptionsAsync(init.FlowId, userInput, cancellationToken);

        return result.Type switch
        {
            "create_entry" => WriteSuccess(settings, result),
            "form" => WriteFormError(settings, result),
            "abort" => WriteAbort(settings, result),
            _ => WriteUnknown(settings, result)
        };
    }

    private static int WriteSuccess(Settings settings, OptionsFlowStep result)
    {
        OutputHelper.WriteResult(settings, result,
            humanOutput: () =>
            {
                AnsiConsole.MarkupLine($"[green]Updated[/] options for [bold]{settings.EntryId.EscapeMarkup()}[/]");
                if (result.Data is { ValueKind: JsonValueKind.Object } data)
                {
                    foreach (var prop in data.EnumerateObject())
                        AnsiConsole.MarkupLine($"  [dim]{prop.Name.EscapeMarkup()}[/] = {prop.Value.ToString().EscapeMarkup()}");
                }
            },
            porcelainOutput: () => Console.WriteLine(settings.EntryId));
        return 0;
    }

    private static int WriteFormError(Settings settings, OptionsFlowStep result)
    {
        var msg = result.Errors is { ValueKind: JsonValueKind.Object } errs && errs.EnumerateObject().Any()
            ? errs.ToString()
            : "Form returned again (validation failed or multi-step flow needs more input).";
        OutputHelper.WriteError(settings, msg);
        return 1;
    }

    private static int WriteAbort(Settings settings, OptionsFlowStep result)
    {
        OutputHelper.WriteError(settings, $"Flow aborted: {result.Reason ?? "unknown reason"}");
        return 1;
    }

    private static int WriteUnknown(Settings settings, OptionsFlowStep result)
    {
        OutputHelper.WriteError(settings, $"Unexpected flow result type: {result.Type}");
        return 1;
    }
}
