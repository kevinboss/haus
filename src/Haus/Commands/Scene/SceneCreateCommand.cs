using System.ComponentModel;
using System.Net;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Scene;

public sealed class SceneCreateCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<SceneCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--data <JSON>")]
        [Description("Full scene configuration as JSON (name, entities, optional icon)")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        [CommandOption("--config-id <ID>")]
        [Description("Config ID for the new scene (default: millisecond timestamp)")]
        public string? ConfigId { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var json = TextInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<SceneConfig>(json);

        var configId = settings.ConfigId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        if (await ConfigIdExists(configId, cancellationToken))
        {
            OutputHelper.WriteError(settings, $"Config ID '{configId}' is already in use. Pick a different --config-id.");
            return 1;
        }

        await api.PostAsync<JsonElement>(
            $"/api/config/scene/config/{configId}", config, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "created", id = configId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{configId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(configId));

        return 0;
    }

    private async Task<bool> ConfigIdExists(string configId, CancellationToken cancellationToken)
    {
        try
        {
            await api.GetAsync<JsonElement>($"/api/config/scene/config/{configId}", cancellationToken);
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
