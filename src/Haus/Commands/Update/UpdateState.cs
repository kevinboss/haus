using System.Text.Json.Serialization;

namespace Haus.Commands.Update;

internal sealed record UpdateAttributes(
    [property: JsonPropertyName("friendly_name")] string? FriendlyName,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("installed_version")] string? InstalledVersion,
    [property: JsonPropertyName("latest_version")] string? LatestVersion,
    [property: JsonPropertyName("skipped_version")] string? SkippedVersion,
    [property: JsonPropertyName("in_progress")] bool InProgress,
    [property: JsonPropertyName("auto_update")] bool AutoUpdate,
    [property: JsonPropertyName("release_url")] string? ReleaseUrl);

internal sealed record UpdateState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] UpdateAttributes Attributes);
