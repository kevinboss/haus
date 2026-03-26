using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Event;

public sealed class EventListCommand(IAuthService auth, IHassApiClient api) : HausCommand<EventListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var events = await api.GetAsync<List<EventType>>("/api/events", cancellationToken);

        if (events is null)
        {
            OutputHelper.WriteError(settings.Json, "Empty response from Home Assistant API.");
            return 1;
        }

        OutputHelper.WriteResult(settings.Json, events, () =>
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Event Type")
                .AddColumn("Listener Count");

            foreach (var evt in events.OrderBy(e => e.Event))
            {
                table.AddRow(
                    evt.Event.EscapeMarkup(),
                    evt.ListenerCount.ToString());
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[dim]{events.Count} event types[/]");
        });

        return 0;
    }
}

internal sealed record EventType(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("listener_count")] int ListenerCount);
