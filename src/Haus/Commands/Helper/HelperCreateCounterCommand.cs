using System.ComponentModel;
using System.Globalization;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateCounterCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<HelperCreateCounterCommand.Settings>(auth)
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

        [CommandOption("--initial <N>")]
        [Description("Initial value")]
        public string? Initial { get; init; }

        [CommandOption("--min <N>")]
        [Description("Minimum value")]
        public int? Min { get; init; }

        [CommandOption("--max <N>")]
        [Description("Maximum value")]
        public int? Max { get; init; }

        [CommandOption("--step <N>")]
        [Description("Step size")]
        public int? Step { get; init; }

        [CommandOption("--restore")]
        [Description("Restore state across HA restarts")]
        public bool Restore { get; init; }
    }

    protected override Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>();
        if (settings.Initial is not null) body["initial"] = int.Parse(settings.Initial, CultureInfo.InvariantCulture);
        if (settings.Min is not null) body["minimum"] = settings.Min.Value;
        if (settings.Max is not null) body["maximum"] = settings.Max.Value;
        if (settings.Step is not null) body["step"] = settings.Step.Value;
        if (settings.Restore) body["restore"] = true;

        return HelperCreator.CreateAsync(ws, HelperKind.Counter, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
