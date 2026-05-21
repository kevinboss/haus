using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Update;

public sealed class UpdateInstallCommand(IAuthService auth, IHassClient client) : HausCommand<UpdateInstallCommand.Settings>(auth)
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

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var preflight = await client.States.GetAsync<UpdateState>(settings.EntityId, cancellationToken);
        if ((preflight.Attributes.SupportedFeatures & UpdateEntityFeature.Install) == 0)
        {
            OutputHelper.WriteError(settings,
                $"{settings.EntityId} is read-only — this update entity does not support install. " +
                "Upgrade via the device or integration itself.");
            return 1;
        }
        if (settings.Version is not null &&
            (preflight.Attributes.SupportedFeatures & UpdateEntityFeature.SpecificVersion) == 0)
        {
            OutputHelper.WriteError(settings,
                $"{settings.EntityId} does not support installing a specific version. Omit --version to install the latest.");
            return 1;
        }
        if (settings.Backup &&
            (preflight.Attributes.SupportedFeatures & UpdateEntityFeature.Backup) == 0)
        {
            OutputHelper.WriteError(settings,
                $"{settings.EntityId} does not support pre-install backups. Omit --backup.");
            return 1;
        }

        var data = new Dictionary<string, object> { ["entity_id"] = settings.EntityId };
        if (!string.IsNullOrEmpty(settings.Version)) data["version"] = settings.Version;
        if (settings.Backup) data["backup"] = true;

        await client.Services.CallAsync("update", "install", data, cancellationToken);

        var state = await client.States.GetAsync<UpdateState>(settings.EntityId, cancellationToken);
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
