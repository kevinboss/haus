using Haus.Auth;
using Haus.Commands;
using Haus.Commands.Automation;
using Haus.Commands.Config;
using Haus.Commands.Entity;
using Haus.Commands.Event;
using Haus.Commands.History;
using Haus.Commands.Log;
using Haus.Commands.Logbook;
using Haus.Commands.Service;
using Haus.Commands.Skill;
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
    config.UseAssemblyInformationalVersion();
    config.AddCommand<LoginCommand>("login")
        .WithDescription("Authenticate with Home Assistant via OAuth2 browser login");
    config.AddCommand<StatusCommand>("status")
        .WithDescription("Check Home Assistant API connectivity");
    config.AddCommand<LogCommand>("log")
        .WithDescription("Show the Home Assistant error log");
    config.AddBranch("logbook", lb =>
    {
        lb.SetDescription("Browse the Home Assistant logbook");
        lb.AddCommand<LogbookListCommand>("list")
            .WithDescription("List logbook entries");
    });
    config.AddBranch("history", hist =>
    {
        hist.SetDescription("Query state history");
        hist.AddCommand<HistoryGetCommand>("get")
            .WithDescription("Get state history for an entity");
    });
    config.AddBranch("config", cfg =>
    {
        cfg.SetDescription("Inspect Home Assistant configuration");
        cfg.AddCommand<ConfigCheckCommand>("check")
            .WithDescription("Validate the current configuration");
    });
    config.AddBranch("automation", auto =>
    {
        auto.SetDescription("Manage automations");
        auto.AddCommand<AutomationGetCommand>("get")
            .WithDescription("Get automation configuration");
        auto.AddCommand<AutomationToggleCommand>("toggle")
            .WithDescription("Toggle an automation on/off");
        auto.AddCommand<AutomationCreateCommand>("create")
            .WithDescription("Create a new automation from a JSON configuration");
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
    config.AddBranch("skill", skill =>
    {
        skill.SetDescription("Install Haus integrations into AI tools");
        skill.AddCommand<SkillInstallCommand>("install")
            .WithDescription("Install the Haus skill into Claude Code (~/.claude/skills/haus/)");
    });
});

return app.Run(args);
