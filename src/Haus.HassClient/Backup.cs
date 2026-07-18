using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.HassClient;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record BackupAgentDetail(
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("protected")] bool Protected);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record BackupInfo(
    [property: JsonPropertyName("backup_id")] string BackupId,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("homeassistant_version")] string? HomeassistantVersion,
    [property: JsonPropertyName("homeassistant_included")] bool HomeassistantIncluded,
    [property: JsonPropertyName("database_included")] bool DatabaseIncluded,
    [property: JsonPropertyName("with_automatic_settings")] bool? WithAutomaticSettings,
    [property: JsonPropertyName("failed_agent_ids")] List<string>? FailedAgentIds,
    [property: JsonPropertyName("agents")] Dictionary<string, BackupAgentDetail>? Agents)
{
    // Size is reported per storage agent; surface the largest (they usually match).
    [JsonIgnore]
    public long? SizeBytes => Agents is { Count: > 0 } ? Agents.Values.Max(a => a.Size) : null;

    [JsonIgnore]
    public double? SizeMb => SizeBytes is { } b ? b / 1024.0 / 1024.0 : null;

    [JsonIgnore]
    public bool Protected => Agents?.Values.Any(a => a.Protected) ?? false;

    [JsonIgnore]
    public IReadOnlyList<string> AgentIds => Agents?.Keys.ToList() ?? [];
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BackupInfoResult(
    [property: JsonPropertyName("backups")] List<BackupInfo>? Backups);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record BackupAgent(
    [property: JsonPropertyName("agent_id")] string AgentId,
    [property: JsonPropertyName("name")] string? Name);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record BackupAgentsResult(
    [property: JsonPropertyName("agents")] List<BackupAgent>? Agents);

// backup/generate returns a job handle; the backup completes in the background.
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record BackupGenerateResult(
    [property: JsonPropertyName("backup_job_id")] string? BackupJobId);
