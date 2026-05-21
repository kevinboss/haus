using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<HelperDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Helper entity ID (e.g. input_boolean.bedtime_lock)")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var domain = settings.EntityId.Split('.', 2)[0];

        var entry = await client.EntityRegistry.GetAsync(settings.EntityId, cancellationToken);
        if (entry?.UniqueId is null)
        {
            OutputHelper.WriteError(settings, $"Could not resolve unique_id for '{settings.EntityId}'.");
            return 1;
        }

        await client.Helper.DeleteAsync(domain, entry.UniqueId, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "deleted", entity_id = settings.EntityId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.EntityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.EntityId));

        return 0;
    }
}
