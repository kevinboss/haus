using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Scene;

public sealed class SceneActivateCommand(IAuthService auth, IHassClient client)
    : HausCommand<SceneActivateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<scene_id>")]
        [Description("Scene entity ID (e.g. scene.movies)")]
        public required string SceneId { get; init; }

        [CommandOption("--transition <SECONDS>")]
        [Description("Transition duration in seconds (applies to compatible entities, e.g. lights)")]
        public double? Transition { get; init; }

        public override ValidationResult Validate() =>
            Transition is < 0
                ? ValidationResult.Error("--transition must be non-negative.")
                : ValidationResult.Success();
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object?> { ["entity_id"] = settings.SceneId };
        if (settings.Transition is not null) payload["transition"] = settings.Transition;

        await client.Services.CallAsync("scene", "turn_on", payload, cancellationToken);

        OutputHelper.WriteResult(settings, new { activated = settings.SceneId },
            () => AnsiConsole.MarkupLine($"[green]Activated[/] [bold]{settings.SceneId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.SceneId));

        return 0;
    }
}
