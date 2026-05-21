using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateBooleanCommand(IAuthService auth, IHassClient client)
    : HausCommand<HelperCreateBooleanCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--name <NAME>")]
        [Description("Display name (HA derives the entity_id from this unless --object-id is set)")]
        public required string Name { get; init; }

        [CommandOption("--object-id <ID>")]
        [Description("Object ID — becomes the entity name (e.g. bedtime_lock → input_boolean.bedtime_lock)")]
        public string? ObjectId { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("MDI icon (e.g. mdi:lock)")]
        public string? Icon { get; init; }

        [CommandOption("--initial <true|false>")]
        [Description("Initial state")]
        public bool? Initial { get; init; }
    }

    protected override Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>();
        if (settings.Initial is not null) body["initial"] = settings.Initial.Value;

        return HelperCreator.CreateAsync(client, HelperKind.Boolean, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
