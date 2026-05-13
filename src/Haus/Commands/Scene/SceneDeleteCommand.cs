using System.ComponentModel;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Scene;

public sealed class SceneDeleteCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<SceneDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<scene_id>")]
        [Description("Scene entity ID (e.g. scene.movies)")]
        public required string SceneId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var state = await api.GetAsync<SceneState>($"/api/states/{settings.SceneId}", cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings,
                $"'{settings.SceneId}' is a runtime scene (created via scene.create service) and is not deletable via this command. It disappears on HA restart.");
            return 1;
        }

        await api.DeleteAsync($"/api/config/scene/config/{state.Attributes.Id}", cancellationToken);

        OutputHelper.WriteResult(settings, new { deleted = settings.SceneId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.SceneId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.SceneId));

        return 0;
    }
}
