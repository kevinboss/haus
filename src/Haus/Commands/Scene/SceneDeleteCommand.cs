using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Scene;

public sealed class SceneDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<SceneDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<scene_id>")]
        [Description("Scene entity ID (e.g. scene.movies)")]
        public required string SceneId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var state = await client.States.GetAsync<SceneState>(settings.SceneId, cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings,
                $"'{settings.SceneId}' is a runtime scene (created via scene.create service) and is not deletable via this command. It disappears on HA restart.");
            return 1;
        }

        await client.SceneConfig.DeleteAsync(state.Attributes.Id, cancellationToken);

        OutputHelper.WriteResult(settings, new { deleted = settings.SceneId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.SceneId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.SceneId));

        return 0;
    }
}
