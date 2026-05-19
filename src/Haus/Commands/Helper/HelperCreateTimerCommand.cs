using System.ComponentModel;
using Haus.Auth;
using Haus.Connection;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateTimerCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<HelperCreateTimerCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--name <NAME>")]
        [Description("Display name")]
        public required string Name { get; init; }

        [CommandOption("--object-id <ID>")]
        [Description("Object ID — becomes the entity name")]
        public string? ObjectId { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("MDI icon")]
        public string? Icon { get; init; }

        [CommandOption("--duration <DUR>")]
        [Description("Timer duration (e.g. 30s, 5m, 1h)")]
        public required string Duration { get; init; }

        [CommandOption("--restore")]
        [Description("Restore running state across HA restarts")]
        public bool Restore { get; init; }
    }

    protected override Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>
        {
            ["duration"] = (int)DurationParser.Parse(settings.Duration).TotalSeconds
        };
        if (settings.Restore) body["restore"] = true;

        return HelperCreator.CreateAsync(ws, HelperKind.Timer, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
