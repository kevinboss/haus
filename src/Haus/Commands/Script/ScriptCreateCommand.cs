using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Script;

public sealed class ScriptCreateCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<ScriptCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--data <JSON>")]
        [Description("Full script configuration as JSON (alias, sequence, optional mode/fields/description)")]
        public required string Data { get; init; }

        [CommandOption("--id <ID>")]
        [Description("Script object ID — becomes the entity name (e.g. notify_all_phones → script.notify_all_phones)")]
        public required string Id { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Data);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var objectId = ScriptGetCommand.StripPrefix(settings.Id);
        var config = ParseTyped<ScriptConfig>(settings.Data);

        await api.PostAsync<JsonElement>(
            $"/api/config/script/config/{objectId}", config, cancellationToken);

        var entityId = $"script.{objectId}";
        OutputHelper.WriteResult(settings, new { action = "created", id = entityId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{entityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(entityId));

        return 0;
    }
}
