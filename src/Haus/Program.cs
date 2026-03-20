using Haus.Auth;
using Haus.Commands;
using Haus.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console.Cli;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .Build())
    .CreateLogger();

var services = new ServiceCollection();
services.AddSerilog();
services.AddSingleton<IAuthService, AuthService>();
services.AddSingleton<IHassConnection, HassConnection>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("haus");
    config.AddCommand<LoginCommand>("login")
        .WithDescription("Authenticate with Home Assistant via OAuth2 browser login");
    config.AddCommand<StatusCommand>("status")
        .WithDescription("Check Home Assistant API connectivity");
});

return app.Run(args);
