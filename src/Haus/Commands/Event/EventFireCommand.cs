using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Event;

public sealed class EventFireCommand(IAuthService auth, IHassApiClient api) : HausCommand<EventFireCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<event_type>")]
        [Description("Event type to fire")]
        public required string EventType { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Event data as JSON")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read event data JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateOptional(Data, FromFile);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var data = ParseJsonData(TextInput.Resolve(settings.Data, settings.FromFile));

        var result = await api.PostAsync<JsonElement>(
            $"/api/events/{settings.EventType}", data, cancellationToken);

        OutputHelper.WriteResult(settings, new { event_type = settings.EventType, result },
            () => AnsiConsole.MarkupLine($"[green]Fired[/] [bold]{settings.EventType.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.EventType));

        return 0;
    }
}
