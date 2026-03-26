using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Service;

public sealed class ServiceListCommand(IAuthService auth, IHassApiClient api) : HausCommand<ServiceListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var domains = await api.GetAsync<List<ServiceDomain>>("/api/services", cancellationToken);

        OutputHelper.WriteResult(settings.Json, domains, () =>
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Domain")
                .AddColumn("Services");

            foreach (var domain in domains.OrderBy(d => d.Domain))
            {
                var serviceNames = string.Join(", ", domain.Services.Keys.Order());
                table.AddRow(
                    domain.Domain.EscapeMarkup(),
                    serviceNames.EscapeMarkup());
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[dim]{domains.Sum(d => d.Services.Count)} services across {domains.Count} domains[/]");
        });

        return 0;
    }
}

internal sealed record ServiceDomain(
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("services")] Dictionary<string, object> Services);
