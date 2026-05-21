using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Event;

public sealed class EventListCommand(IAuthService auth, IHassClient client) : HausCommand<EventListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var events = await client.Events.ListTypesAsync(cancellationToken);

        OutputHelper.WriteResult(settings, events,
            () =>
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
            },
            () =>
            {
                OutputHelper.WriteColumns(
                    ["EVENT TYPE", "LISTENER COUNT"],
                    events.OrderBy(e => e.Event).Select(e => new[]
                    {
                        e.Event, e.ListenerCount.ToString()
                    }));
            });

        return 0;
    }
}
