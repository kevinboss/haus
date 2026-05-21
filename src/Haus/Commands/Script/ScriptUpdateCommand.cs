using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Script;

public sealed class ScriptUpdateCommand(IAuthService auth, IHassClient client)
    : HausCommand<ScriptUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<script_id>")]
        [Description("Script entity ID (e.g. script.notify_all_phones)")]
        public required string ScriptId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Full script configuration as JSON")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var objectId = ScriptGetCommand.StripPrefix(settings.ScriptId);
        var json = TextInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<ScriptConfig>(json);

        await client.ScriptConfig.SaveAsync(objectId, config, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "updated", id = settings.ScriptId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.ScriptId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.ScriptId));

        return 0;
    }
}
