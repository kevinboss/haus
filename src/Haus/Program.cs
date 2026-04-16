using Haus.Auth;
using Haus.Commands;
using Haus.Commands.Automation;
using Haus.Commands.Entity;
using Haus.Commands.Event;
using Haus.Commands.Service;
using Haus.Commands.State;
using Haus.Commands.Update;
using Haus.Connection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<IAuthService, AuthService>();
services.AddSingleton<IHassApiClient, HassApiClient>();
services.AddSingleton<IHassWebSocketClient, HassWebSocketClient>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("haus");
    config.AddCommand<LoginCommand>("login")
        .WithDescription("Authenticate with Home Assistant via OAuth2 browser login");
    config.AddCommand<StatusCommand>("status")
        .WithDescription("Check Home Assistant API connectivity");
    config.AddBranch("automation", auto =>
    {
        auto.SetDescription("Manage automations");
        auto.AddCommand<AutomationGetCommand>("get")
            .WithDescription("Get automation configuration");
        auto.AddCommand<AutomationToggleCommand>("toggle")
            .WithDescription("Toggle an automation on/off");
        auto.AddCommand<AutomationUpdateCommand>("update")
            .WithDescription("Update an automation's configuration");
        auto.AddCommand<AutomationDeleteCommand>("delete")
            .WithDescription("Delete an automation");
    });
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
    config.AddBranch("entity", ent =>
    {
        ent.SetDescription("Manage entity registry");
        ent.AddCommand<EntityRenameCommand>("rename")
            .WithDescription("Rename an entity's display name");
    });
    config.AddBranch("service", svc =>
    {
        svc.SetDescription("Call Home Assistant services");
        svc.AddCommand<ServiceListCommand>("list")
            .WithDescription("List available services by domain");
        svc.AddCommand<ServiceCallCommand>("call")
            .WithDescription("Call a service (e.g. light.turn_on, vacuum.start)");
    });
    config.AddBranch("update", upd =>
    {
        upd.SetDescription("Check for available updates");
        upd.AddCommand<UpdateListCommand>("list")
            .WithDescription("List update entities and their availability");
        upd.AddCommand<UpdateInstallCommand>("install")
            .WithDescription("Install an available update");
    });
});

return app.Run(args);
