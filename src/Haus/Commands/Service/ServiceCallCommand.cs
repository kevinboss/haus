using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Service;

public sealed class ServiceCallCommand(IAuthService auth, IHassApiClient api) : HausCommand<ServiceCallCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<domain.service>")]
        [Description("Service to call (e.g. light.turn_on, vacuum.start)")]
        public required string Service { get; init; }

        [CommandOption("--entity <ENTITY_ID>")]
        [Description("Target entity ID (shorthand for --data '{\"entity_id\": \"...\"}'")]
        public string? EntityId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Service data as JSON")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read service data JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public string Domain => Service[..Service.IndexOf('.')];
        public string ServiceName => Service[(Service.IndexOf('.') + 1)..];

        public override ValidationResult Validate()
        {
            var dot = Service.IndexOf('.');
            if (dot <= 0 || dot >= Service.Length - 1)
                return ValidationResult.Error("Service must be in domain.service format (e.g. light.turn_on).");

            return JsonInput.ValidateOptional(Data, FromFile);
        }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var domain = settings.Domain;
        var service = settings.ServiceName;

        var data = ParseJsonData(TextInput.Resolve(settings.Data, settings.FromFile));
        if (settings.EntityId is not null)
        {
            data ??= [];
            data["entity_id"] = settings.EntityId;
        }

        var result = await api.PostAsync<JsonElement>(
            $"/api/services/{domain}/{service}", data, cancellationToken);

        OutputHelper.WriteResult(settings, new { domain, service, result },
            () => AnsiConsole.MarkupLine($"[green]Called[/] [bold]{domain}.{service}[/]"),
            () => Console.WriteLine($"{domain}.{service}"));

        return 0;
    }
}
