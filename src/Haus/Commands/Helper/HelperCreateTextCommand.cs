using System.ComponentModel;
using Haus.Auth;
using Haus.Connection;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperCreateTextCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<HelperCreateTextCommand.Settings>(auth)
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

        [CommandOption("--initial <VALUE>")]
        [Description("Initial value")]
        public string? Initial { get; init; }

        [CommandOption("--min <LENGTH>")]
        [Description("Minimum length")]
        public int? Min { get; init; }

        [CommandOption("--max <LENGTH>")]
        [Description("Maximum length")]
        public int? Max { get; init; }

        [CommandOption("--mode <MODE>")]
        [Description("Display mode: text | password")]
        public string? Mode { get; init; }

        [CommandOption("--pattern <REGEX>")]
        [Description("Regex pattern the value must match")]
        public string? Pattern { get; init; }
    }

    protected override Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>();
        if (settings.Initial is not null) body["initial"] = settings.Initial;
        if (settings.Min is not null) body["min"] = settings.Min.Value;
        if (settings.Max is not null) body["max"] = settings.Max.Value;
        if (settings.Mode is not null) body["mode"] = settings.Mode;
        if (settings.Pattern is not null) body["pattern"] = settings.Pattern;

        return HelperCreator.CreateAsync(ws, HelperKind.Text, settings.Name, settings.ObjectId, settings.Icon, body, settings, cancellationToken);
    }
}
