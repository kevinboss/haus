using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Update;

public sealed class UpdateInstallCommand(IAuthService auth, IHassApiClient api) : HausCommand<UpdateInstallCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Update entity ID (e.g. update.home_assistant_core_update)")]
        public required string EntityId { get; init; }

        [CommandOption("--version <VERSION>")]
        [Description("Install a specific version (defaults to latest)")]
        public string? Version { get; init; }

        [CommandOption("--backup")]
        [Description("Create a backup before installing (if supported by the integration)")]
        public bool Backup { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, object> { ["entity_id"] = settings.EntityId };
        if (!string.IsNullOrEmpty(settings.Version)) data["version"] = settings.Version;
        if (settings.Backup) data["backup"] = true;

        await api.PostAsync<JsonElement>("/api/services/update/install", data, cancellationToken);

        var state = await api.GetAsync<UpdateState>($"/api/states/{settings.EntityId}", cancellationToken);
        var title = state.Attributes.Title ?? state.Attributes.FriendlyName ?? settings.EntityId;
        var target = settings.Version ?? state.Attributes.LatestVersion ?? "latest";

        OutputHelper.WriteResult(settings,
            new
            {
                entity_id = settings.EntityId,
                installed_version = state.Attributes.InstalledVersion,
                target_version = target,
                in_progress = state.Attributes.InProgress
            },
            () => AnsiConsole.MarkupLine(
                $"[green]Install requested[/] [bold]{title.EscapeMarkup()}[/] → [bold]{target.EscapeMarkup()}[/]"),
            () => OutputHelper.WriteKeyValue(settings.EntityId, target));

        return 0;
    }
}
