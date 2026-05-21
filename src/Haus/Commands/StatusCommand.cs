using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands;

public sealed class StatusCommand(IAuthService auth, IHassClient client) : HausCommand<StatusCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var apiStatus = await client.Status.GetAsync(cancellationToken);

        OutputHelper.WriteResult(settings, new { version = apiStatus.Version, message = apiStatus.Message },
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Property")
                    .AddColumn("Value");

                table.AddRow("[bold]Version[/]", (apiStatus.Version ?? "").EscapeMarkup());
                table.AddRow("[bold]Message[/]", apiStatus.Message.EscapeMarkup());

                AnsiConsole.Write(table);
            },
            () =>
            {
                OutputHelper.WriteKeyValue("version", apiStatus.Version ?? "");
                OutputHelper.WriteKeyValue("message", apiStatus.Message);
            });

        return 0;
    }
}
