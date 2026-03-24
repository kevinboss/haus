using System.ComponentModel;
using System.Text.Json;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.State;

public sealed class StateSetCommand(IHassApiClient api) : HausCommand<StateSetCommand.Settings>(api)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID (e.g. sensor.custom_value)")]
        public required string EntityId { get; init; }

        [CommandArgument(1, "<state>")]
        [Description("State value to set")]
        public required string State { get; init; }

        [CommandOption("--attributes <JSON>")]
        [Description("Entity attributes as JSON")]
        public string? Attributes { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Attributes);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object> { ["state"] = settings.State };

        if (settings.Attributes is not null)
        {
            var attrs = JsonSerializer.Deserialize<Dictionary<string, object>>(settings.Attributes);
            if (attrs is not null)
                payload["attributes"] = attrs;
        }

        var result = await Api.PostAsync<EntityState>(
            $"/api/states/{settings.EntityId}", payload, cancellationToken);

        OutputHelper.WriteResult(settings.Json, result, () =>
        {
            AnsiConsole.MarkupLine($"[green]Set[/] [bold]{settings.EntityId.EscapeMarkup()}[/] to [bold]{result.State.EscapeMarkup()}[/]");
        });

        return 0;
    }
}
