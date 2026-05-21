using Haus.Auth;
using Haus.Commands;
using Haus.Commands.Automation;
using Haus.Commands.Config;
using Haus.Commands.Dashboard;
using Haus.Commands.Entity;
using Haus.Commands.Event;
using Haus.Commands.Helper;
using Haus.Commands.History;
using Haus.Commands.Log;
using Haus.Commands.Logbook;
using Haus.Commands.Scene;
using Haus.Commands.Script;
using Haus.Commands.Service;
using Haus.Commands.Skill;
using Haus.Commands.State;
using Haus.Commands.Template;
using Haus.Commands.Update;
using Haus.Commands.Zone;
using Haus.HassClient;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<AuthService>();
services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<AuthService>());
services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<AuthService>());
services.AddHassClient();

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
    config.AddCommand<TemplateCommand>("template")
        .WithDescription("Render a Jinja2 template against current state");
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
        auto.AddCommand<AutomationListCommand>("list")
            .WithDescription("List all automations");
        auto.AddCommand<AutomationGetCommand>("get")
            .WithDescription("Get automation configuration");
        auto.AddCommand<AutomationTraceCommand>("trace")
            .WithDescription("View recent execution traces");
        auto.AddCommand<AutomationToggleCommand>("toggle")
            .WithDescription("Toggle an automation on/off");
        auto.AddCommand<AutomationEnableCommand>("enable")
            .WithDescription("Enable an automation");
        auto.AddCommand<AutomationDisableCommand>("disable")
            .WithDescription("Disable an automation");
        auto.AddCommand<AutomationCreateCommand>("create")
            .WithDescription("Create a new automation from a JSON configuration");
        auto.AddCommand<AutomationUpdateCommand>("update")
            .WithDescription("Update an automation's configuration");
        auto.AddCommand<AutomationDeleteCommand>("delete")
            .WithDescription("Delete an automation");
    });
    config.AddBranch("script", scr =>
    {
        scr.SetDescription("Manage scripts");
        scr.AddCommand<ScriptListCommand>("list")
            .WithDescription("List all scripts");
        scr.AddCommand<ScriptGetCommand>("get")
            .WithDescription("Get script configuration");
        scr.AddCommand<ScriptCreateCommand>("create")
            .WithDescription("Create a new script from a JSON configuration");
        scr.AddCommand<ScriptUpdateCommand>("update")
            .WithDescription("Update a script's configuration");
        scr.AddCommand<ScriptDeleteCommand>("delete")
            .WithDescription("Delete a script");
    });
    config.AddBranch("scene", scn =>
    {
        scn.SetDescription("Manage scenes (config and runtime)");
        scn.AddCommand<SceneListCommand>("list")
            .WithDescription("List all scenes (config + runtime)");
        scn.AddCommand<SceneGetCommand>("get")
            .WithDescription("Get scene details");
        scn.AddCommand<SceneCreateCommand>("create")
            .WithDescription("Create a new scene from a JSON configuration");
        scn.AddCommand<SceneUpdateCommand>("update")
            .WithDescription("Update a config scene");
        scn.AddCommand<SceneDeleteCommand>("delete")
            .WithDescription("Delete a config scene");
        scn.AddCommand<SceneActivateCommand>("activate")
            .WithDescription("Activate a scene (wraps scene.turn_on)");
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
        ent.AddCommand<EntityListCommand>("list")
            .WithDescription("List all registered entities (including disabled/hidden)");
        ent.AddCommand<EntityGetCommand>("get")
            .WithDescription("Show registry metadata for an entity");
        ent.AddCommand<EntityRenameCommand>("rename")
            .WithDescription("Rename an entity's display name");
        ent.AddCommand<EntityRenameIdCommand>("rename-id")
            .WithDescription("Rename an entity's ID (HA rewires references atomically)");
        ent.AddCommand<EntityUpdateCommand>("update")
            .WithDescription("Update an entity's registry fields (name, icon, area, disable, hide, new ID)");
        ent.AddCommand<EntityDeleteCommand>("delete")
            .WithDescription("Remove an entity from the registry");
    });
    config.AddBranch("helper", h =>
    {
        h.SetDescription("Create and manage UI helpers (input_boolean, counter, timer, ...)");
        h.AddCommand<HelperListCommand>("list")
            .WithDescription("List all helpers across input_*, counter, timer");
        h.AddCommand<HelperDeleteCommand>("delete")
            .WithDescription("Delete a helper");
        h.AddCommand<HelperCreateBooleanCommand>("create-boolean")
            .WithDescription("Create an input_boolean");
        h.AddCommand<HelperCreateTextCommand>("create-text")
            .WithDescription("Create an input_text");
        h.AddCommand<HelperCreateNumberCommand>("create-number")
            .WithDescription("Create an input_number");
        h.AddCommand<HelperCreateSelectCommand>("create-select")
            .WithDescription("Create an input_select");
        h.AddCommand<HelperCreateDatetimeCommand>("create-datetime")
            .WithDescription("Create an input_datetime");
        h.AddCommand<HelperCreateCounterCommand>("create-counter")
            .WithDescription("Create a counter");
        h.AddCommand<HelperCreateTimerCommand>("create-timer")
            .WithDescription("Create a timer");
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
    config.AddBranch("dashboard", dash =>
    {
        dash.SetDescription("Manage Lovelace dashboards");
        dash.AddCommand<DashboardListCommand>("list")
            .WithDescription("List dashboards");
        dash.AddCommand<DashboardGetCommand>("get")
            .WithDescription("Get dashboard registry props and view summary");
        dash.AddCommand<DashboardCreateCommand>("create")
            .WithDescription("Create a new storage-mode dashboard");
        dash.AddCommand<DashboardUpdateCommand>("update")
            .WithDescription("Update a dashboard's registry props (title, icon, sidebar, admin)");
        dash.AddCommand<DashboardDeleteCommand>("delete")
            .WithDescription("Delete a dashboard");
        dash.AddBranch("config", cfg =>
        {
            cfg.SetDescription("Read and write dashboard view configuration");
            cfg.AddCommand<DashboardConfigGetCommand>("get")
                .WithDescription("Get dashboard config (views/cards)");
            cfg.AddCommand<DashboardConfigSaveCommand>("save")
                .WithDescription("Replace a dashboard's config (views/cards)");
        });
    });
    config.AddBranch("zone", zone =>
    {
        zone.SetDescription("Manage geofence zones");
        zone.AddCommand<ZoneListCommand>("list")
            .WithDescription("List all zones");
        zone.AddCommand<ZoneGetCommand>("get")
            .WithDescription("Show full zone details");
        zone.AddCommand<ZoneUpdateCommand>("update")
            .WithDescription("Update a zone (radius, coordinates, icon, passive flag)");
    });
    config.AddBranch("skill", skill =>
    {
        skill.SetDescription("Install Haus integrations into AI tools");
        skill.AddCommand<SkillInstallCommand>("install")
            .WithDescription("Install the Haus skill into Claude Code (~/.claude/skills/haus/)");
    });
});

return app.Run(args);
