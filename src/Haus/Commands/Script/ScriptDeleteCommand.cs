using System.ComponentModel;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Script;

public sealed class ScriptDeleteCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<ScriptDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<script_id>")]
        [Description("Script entity ID (e.g. script.notify_all_phones)")]
        public required string ScriptId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var objectId = ScriptGetCommand.StripPrefix(settings.ScriptId);
        await api.DeleteAsync($"/api/config/script/config/{objectId}", cancellationToken);

        OutputHelper.WriteResult(settings, new { deleted = settings.ScriptId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.ScriptId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.ScriptId));

        return 0;
    }
}
