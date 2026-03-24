using System.ComponentModel;
using System.Text.Json;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Event;

public sealed class EventFireCommand(IHassApiClient api) : HausCommand<EventFireCommand.Settings>(api)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<event_type>")]
        [Description("Event type to fire")]
        public required string EventType { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Event data as JSON")]
        public string? Data { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Data);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var data = ParseJsonData(settings.Data);

        var result = await Api.PostAsync<JsonElement>(
            $"/api/events/{settings.EventType}", data, cancellationToken);

        OutputHelper.WriteResult(settings.Json, new { event_type = settings.EventType, result }, () =>
        {
            AnsiConsole.MarkupLine($"[green]Fired[/] [bold]{settings.EventType.EscapeMarkup()}[/]");
        });

        return 0;
    }
}
