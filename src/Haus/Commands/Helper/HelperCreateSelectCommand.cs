using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateSelectCommand(IAuthService auth, IHassClient client)
    : HausCommand<HelperCreateSelectCommand.Settings>(auth)
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

        [CommandOption("--options <CSV>")]
        [Description("Comma-separated list of options")]
        public required string Options { get; init; }

        [CommandOption("--initial <VALUE>")]
        [Description("Initial option (must be one of --options)")]
        public string? Initial { get; init; }
    }

    protected override Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var options = settings.Options.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var body = new Dictionary<string, object?>
        {
            ["options"] = options
        };
        if (settings.Initial is not null) body["initial"] = settings.Initial;

        return HelperCreator.CreateAsync(client, HelperKind.Select, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
