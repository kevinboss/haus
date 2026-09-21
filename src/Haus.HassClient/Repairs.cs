using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haus.HassClient;

public sealed record RepairIssue(
    [property: JsonPropertyName("issue_id")] string IssueId,
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("created")] DateTimeOffset? Created,
    [property: JsonPropertyName("ignored")] bool Ignored,
    [property: JsonPropertyName("dismissed_version")] string? DismissedVersion,
    [property: JsonPropertyName("is_fixable")] bool IsFixable,
    [property: JsonPropertyName("issue_domain")] string? IssueDomain,
    [property: JsonPropertyName("severity")] string? Severity,
    [property: JsonPropertyName("breaks_in_ha_version")] string? BreaksInHaVersion,
    [property: JsonPropertyName("learn_more_url")] string? LearnMoreUrl,
    [property: JsonPropertyName("translation_key")] string? TranslationKey,
    [property: JsonPropertyName("translation_placeholders")] JsonElement? TranslationPlaceholders)
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonIgnore]
    public string LocalizationKey => TranslationKey ?? IssueId;
}
