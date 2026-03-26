using Haus.Auth;
using Haus.Commands;
using Haus.Commands.Event;
using Haus.Commands.Service;
using Haus.Commands.State;
using Haus.Connection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<IAuthService, AuthService>();
services.AddSingleton<IHassApiClient, HassApiClient>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("haus");
    config.AddCommand<LoginCommand>("login")
        .WithDescription("Authenticate with Home Assistant via OAuth2 browser login");
    config.AddCommand<StatusCommand>("status")
        .WithDescription("Check Home Assistant API connectivity");
    config.AddBranch("state", state =>
    {
        state.SetDescription("Manage entity states");
        state.AddCommand<StateListCommand>("list")
            .WithDescription("List all entity states");
        state.AddCommand<StateGetCommand>("get")
            .WithDescription("Get state and attributes of an entity");
        state.AddCommand<StateSetCommand>("set")
            .WithDescription("Set state of an entity");
        state.AddCommand<StateDeleteCommand>("delete")
            .WithDescription("Remove an entity from the state machine");
    });
    config.AddBranch("event", evt =>
    {
        evt.SetDescription("List and fire events");
        evt.AddCommand<EventListCommand>("list")
            .WithDescription("List event types");
        evt.AddCommand<EventFireCommand>("fire")
            .WithDescription("Fire a custom event");
    });
    config.AddBranch("service", svc =>
    {
        svc.SetDescription("Call Home Assistant services");
        svc.AddCommand<ServiceListCommand>("list")
            .WithDescription("List available services by domain");
        svc.AddCommand<ServiceCallCommand>("call")
            .WithDescription("Call a service (e.g. light.turn_on, vacuum.start)");
    });
});

return app.Run(args);
