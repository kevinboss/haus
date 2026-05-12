using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Script;

public sealed class ScriptUpdateCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<ScriptUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<script_id>")]
        [Description("Script entity ID (e.g. script.notify_all_phones)")]
        public required string ScriptId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Full script configuration as JSON")]
        public required string Data { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Data);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var objectId = ScriptGetCommand.StripPrefix(settings.ScriptId);
        var config = ParseTyped<ScriptConfig>(settings.Data);

        var result = await api.PostAsync<JsonElement>(
            $"/api/config/script/config/{objectId}", config, cancellationToken);

        OutputHelper.WriteResult(settings, result,
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.ScriptId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.ScriptId));

        return 0;
    }
}
