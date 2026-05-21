using System.ComponentModel;
using Haus.HassClient;
using System.Globalization;
using Haus.Auth;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateNumberCommand(IAuthService auth, IHassClient client)
    : HausCommand<HelperCreateNumberCommand.Settings>(auth)
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

        [CommandOption("--min <N>")]
        [Description("Minimum value")]
        public required double Min { get; init; }

        [CommandOption("--max <N>")]
        [Description("Maximum value")]
        public required double Max { get; init; }

        [CommandOption("--step <N>")]
        [Description("Step size")]
        public double? Step { get; init; }

        [CommandOption("--initial <N>")]
        [Description("Initial value")]
        public string? Initial { get; init; }

        [CommandOption("--mode <MODE>")]
        [Description("Display mode: box | slider")]
        public string? Mode { get; init; }

        [CommandOption("--unit <UNIT>")]
        [Description("Unit of measurement")]
        public string? Unit { get; init; }
    }

    protected override Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>
        {
            ["min"] = settings.Min,
            ["max"] = settings.Max
        };
        if (settings.Step is not null) body["step"] = settings.Step.Value;
        if (settings.Initial is not null) body["initial"] = double.Parse(settings.Initial, CultureInfo.InvariantCulture);
        if (settings.Mode is not null) body["mode"] = settings.Mode;
        if (settings.Unit is not null) body["unit_of_measurement"] = settings.Unit;

        return HelperCreator.CreateAsync(client, HelperKind.Number, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
