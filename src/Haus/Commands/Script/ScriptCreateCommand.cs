using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
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
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        [CommandOption("--object-id <ID>")]
        [Description("Script object ID — becomes the entity name (e.g. notify_all_phones → script.notify_all_phones)")]
        public required string ObjectId { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var objectId = ScriptGetCommand.StripPrefix(settings.ObjectId);
        var json = TextInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<ScriptConfig>(json);

        await api.PostAsync<JsonElement>(
            $"/api/config/script/config/{objectId}", config, cancellationToken);

        var entityId = $"script.{objectId}";
        OutputHelper.WriteResult(settings, new { action = "created", id = entityId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{entityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(entityId));

        return 0;
    }
}
