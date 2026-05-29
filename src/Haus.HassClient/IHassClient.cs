namespace Haus.HassClient;

/// <summary>
/// Single entry point to the Home Assistant API. Methods are grouped by HA topic.
/// Transport (REST vs WebSocket) is hidden — pick the accessor that matches what you want to do.
/// </summary>
public interface IHassClient
{
    // REST topics
    IStatusClient Status { get; }
    IStatesClient States { get; }
    IEventsClient Events { get; }
    IServicesClient Services { get; }
    ITemplateClient Template { get; }
    IConfigClient Config { get; }
    IAutomationConfigClient AutomationConfig { get; }
    IScriptConfigClient ScriptConfig { get; }
    ISceneConfigClient SceneConfig { get; }
    IHistoryClient History { get; }
    ILogbookClient Logbook { get; }

    // WebSocket topics
    IEntityRegistryClient EntityRegistry { get; }
    ILovelaceClient Lovelace { get; }
    ITraceClient Trace { get; }
    ISystemLogClient SystemLog { get; }
    IStatisticsClient Statistics { get; }
    IHelperClient Helper { get; }
    IZoneClient Zone { get; }
    IIntegrationClient Integration { get; }
}
