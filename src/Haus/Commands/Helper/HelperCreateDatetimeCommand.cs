using System.ComponentModel;
using Haus.Auth;
using Haus.Ws;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateDatetimeCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<HelperCreateDatetimeCommand.Settings>(auth)
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

        [CommandOption("--has-date")]
        [Description("Include a date component")]
        public bool HasDate { get; init; }

        [CommandOption("--has-time")]
        [Description("Include a time component")]
        public bool HasTime { get; init; }

        [CommandOption("--initial <ISO>")]
        [Description("Initial value (ISO timestamp, e.g. 2026-05-19T08:00:00)")]
        public string? Initial { get; init; }

        public override ValidationResult Validate() =>
            HasDate || HasTime
                ? ValidationResult.Success()
                : ValidationResult.Error("Pass --has-date and/or --has-time.");
    }

    protected override Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>
        {
            ["has_date"] = settings.HasDate,
            ["has_time"] = settings.HasTime
        };
        if (settings.Initial is not null) body["initial"] = settings.Initial;

        return HelperCreator.CreateAsync(ws, HelperKind.Datetime, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
