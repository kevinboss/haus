using System.Text.Json.Serialization;

namespace Haus.Ws;

public sealed record NewDashboard(
    string UrlPath,
    string Title,
    string? Icon = null,
    bool ShowInSidebar = true,
    bool RequireAdmin = false);

public sealed record DashboardUpdate(
    string? Title = null,
    string? Icon = null,
    bool? ShowInSidebar = null,
    bool? RequireAdmin = null);

public sealed record DashboardRegistryEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("url_path")] string UrlPath,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("show_in_sidebar")] bool ShowInSidebar,
    [property: JsonPropertyName("require_admin")] bool RequireAdmin,
    [property: JsonPropertyName("mode")] string Mode);
