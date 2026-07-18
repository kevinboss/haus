
namespace Haus.HassClient;

public sealed class HassClient(IHassApiClient rest, IHassWebSocketClient ws) : IHassClient
{
    public IStatusClient Status { get; } = new StatusClient(rest);
    public IStatesClient States { get; } = new StatesClient(rest);
    public IEventsClient Events { get; } = new EventsClient(rest);
    public IServicesClient Services { get; } = new ServicesClient(rest);
    public ITemplateClient Template { get; } = new TemplateClient(rest);
    public IConfigClient Config { get; } = new ConfigClient(rest);
    public IAutomationConfigClient AutomationConfig { get; } = new AutomationConfigClient(rest);
    public IScriptConfigClient ScriptConfig { get; } = new ScriptConfigClient(rest);
    public ISceneConfigClient SceneConfig { get; } = new SceneConfigClient(rest);
    public IHistoryClient History { get; } = new HistoryClient(rest);
    public ILogbookClient Logbook { get; } = new LogbookClient(rest);

    public IEntityRegistryClient EntityRegistry { get; } = new EntityRegistryClient(ws);
    public IAreaRegistryClient Area { get; } = new AreaRegistryClient(ws);
    public ILabelRegistryClient Label { get; } = new LabelRegistryClient(ws);
    public IDeviceRegistryClient Device { get; } = new DeviceRegistryClient(ws);
    public ILovelaceClient Lovelace { get; } = new LovelaceClient(ws);
    public ITraceClient Trace { get; } = new TraceClient(ws);
    public ISystemLogClient SystemLog { get; } = new SystemLogClient(ws);
    public IStatisticsClient Statistics { get; } = new StatisticsClient(ws);
    public IHelperClient Helper { get; } = new HelperClient(ws);
    public IZoneClient Zone { get; } = new ZoneClient(ws);
    public IIntegrationClient Integration { get; } = new IntegrationClient(ws, rest);
}
