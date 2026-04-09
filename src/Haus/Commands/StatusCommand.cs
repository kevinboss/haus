using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands;

public sealed class StatusCommand(IAuthService auth, IHassApiClient api) : HausCommand<StatusCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var apiStatus = await api.GetAsync<ApiStatusResponse>("/api/", cancellationToken);

        OutputHelper.WriteResult(settings, new { version = apiStatus.Version, message = apiStatus.Message },
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Property")
                    .AddColumn("Value");

                table.AddRow("[bold]Version[/]", apiStatus.Version.EscapeMarkup());
                table.AddRow("[bold]Message[/]", apiStatus.Message.EscapeMarkup());

                AnsiConsole.Write(table);
            },
            () =>
            {
                OutputHelper.WriteKeyValue("version", apiStatus.Version);
                OutputHelper.WriteKeyValue("message", apiStatus.Message);
            });

        return 0;
    }

    private sealed record ApiStatusResponse(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("version")] string Version);
}
