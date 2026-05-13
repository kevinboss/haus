using System.ComponentModel;
using System.Text.RegularExpressions;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed partial class EntityRenameIdCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<EntityRenameIdCommand.Settings>(auth)
{
    [GeneratedRegex(@"^[a-z0-9_]+\.[a-z0-9_]+$")]
    private static partial Regex EntityIdRegex { get; }

    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<old_entity_id>")]
        [Description("Current entity ID (e.g. automation.sunday_morning_cleaning)")]
        public required string OldEntityId { get; init; }

        [CommandArgument(1, "<new_entity_id>")]
        [Description("Replacement entity ID (must share the same domain)")]
        public required string NewEntityId { get; init; }

        public override ValidationResult Validate()
        {
            if (!EntityIdRegex.IsMatch(NewEntityId))
                return ValidationResult.Error(
                    $"'{NewEntityId}' is not a valid entity ID. Expected <domain>.<object_id> with lowercase letters, digits, and underscores only.");

            var oldDomain = OldEntityId.Split('.', 2)[0];
            var newDomain = NewEntityId.Split('.', 2)[0];
            if (oldDomain != newDomain)
                return ValidationResult.Error(
                    $"Domain prefix must match: cannot rename {oldDomain}.* to {newDomain}.*.");

            if (OldEntityId == NewEntityId)
                return ValidationResult.Error("Old and new entity IDs are identical.");

            return ValidationResult.Success();
        }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        await ws.SendCommandAsync(new
        {
            type = EntityRegistryCommands.Update,
            entity_id = settings.OldEntityId,
            new_entity_id = settings.NewEntityId
        }, cancellationToken);

        OutputHelper.WriteResult(settings,
            new { action = "renamed", from = settings.OldEntityId, to = settings.NewEntityId },
            () => AnsiConsole.MarkupLine(
                $"[green]Renamed[/] [bold]{settings.OldEntityId.EscapeMarkup()}[/] → [bold]{settings.NewEntityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine($"{settings.OldEntityId}\t{settings.NewEntityId}"));

        return 0;
    }
}
