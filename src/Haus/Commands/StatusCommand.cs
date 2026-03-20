using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands;

public sealed class StatusCommand(IAuthService authService) : AsyncCommand<StatusCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!authService.IsLoggedIn)
        {
            OutputHelper.WriteError(settings.Json, "Not logged in. Run `haus login` or set HASS_URL/HASS_TOKEN.");
            return 1;
        }

        try
        {
            var (url, token) = await authService.GetAccessTokenAsync(cancellationToken);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var apiStatus = await httpClient.GetFromJsonAsync<ApiStatusResponse>(
                $"{url}/api/", cancellationToken: cancellationToken);

            if (apiStatus is null)
            {
                OutputHelper.WriteError(settings.Json, "Empty response from Home Assistant API.");
                return 1;
            }

            OutputHelper.WriteResult(settings.Json, new { url, version = apiStatus.Version, message = apiStatus.Message }, () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Property")
                    .AddColumn("Value");

                table.AddRow("[bold]URL[/]", url.EscapeMarkup());
                table.AddRow("[bold]Version[/]", apiStatus.Version.EscapeMarkup());
                table.AddRow("[bold]Message[/]", apiStatus.Message.EscapeMarkup());

                AnsiConsole.Write(table);
            });

            return 0;
        }
        catch (Exception ex)
        {
            OutputHelper.WriteError(settings.Json, ex);
            return 1;
        }
    }

    private sealed record ApiStatusResponse(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("version")] string Version);
}
