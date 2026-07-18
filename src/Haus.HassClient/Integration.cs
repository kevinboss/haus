using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haus.HassClient;

public sealed record ConfigEntry(
    [property: JsonPropertyName("entry_id")] string EntryId,
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("supports_options")] bool SupportsOptions,
    [property: JsonPropertyName("supports_remove_device")] bool SupportsRemoveDevice,
    [property: JsonPropertyName("supports_unload")] bool SupportsUnload,
    [property: JsonPropertyName("supports_reconfigure")] bool? SupportsReconfigure,
    [property: JsonPropertyName("disabled_by")] string? DisabledBy,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("pref_disable_new_entities")] bool PrefDisableNewEntities,
    [property: JsonPropertyName("pref_disable_polling")] bool PrefDisablePolling);

public sealed record ConfigEntryOperationResult(
    [property: JsonPropertyName("require_restart")] bool RequireRestart);

public sealed record OptionsFlowStep(
    [property: JsonPropertyName("flow_id")] string FlowId,
    [property: JsonPropertyName("handler")] JsonElement? Handler,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("step_id")] string? StepId,
    [property: JsonPropertyName("data_schema")] JsonElement? DataSchema,
    [property: JsonPropertyName("errors")] JsonElement? Errors,
    [property: JsonPropertyName("description_placeholders")] JsonElement? DescriptionPlaceholders,
    [property: JsonPropertyName("last_step")] bool? LastStep,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("data")] JsonElement? Data,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("reason")] string? Reason);
