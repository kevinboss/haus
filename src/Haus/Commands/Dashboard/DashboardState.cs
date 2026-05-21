using System.Text.Json.Serialization;

namespace Haus.Commands.Dashboard;

internal static class LovelaceCommands
{
    public const string DashboardsList = "lovelace/dashboards/list";
    public const string DashboardsCreate = "lovelace/dashboards/create";
    public const string DashboardsUpdate = "lovelace/dashboards/update";
    public const string DashboardsDelete = "lovelace/dashboards/delete";
    public const string Config = "lovelace/config";
    public const string ConfigSave = "lovelace/config/save";
}

internal sealed record DashboardRegistryEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("url_path")] string UrlPath,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("show_in_sidebar")] bool ShowInSidebar,
    [property: JsonPropertyName("require_admin")] bool RequireAdmin,
    [property: JsonPropertyName("mode")] string Mode);
