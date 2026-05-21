using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Scene;

public sealed class SceneUpdateCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<SceneUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<scene_id>")]
        [Description("Scene entity ID (e.g. scene.movies)")]
        public required string SceneId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Full scene configuration as JSON")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var state = await api.GetAsync<SceneState>($"/api/states/{settings.SceneId}", cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings,
                $"'{settings.SceneId}' is a runtime scene (created via scene.create service) and cannot be edited.");
            return 1;
        }

        var json = TextInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<SceneConfig>(json);

        await api.PostAsync<JsonElement>(
            $"/api/config/scene/config/{state.Attributes.Id}", config, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "updated", id = settings.SceneId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.SceneId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.SceneId));

        return 0;
    }
}
