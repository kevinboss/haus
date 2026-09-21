using System.Text.Json;

namespace Haus.HassClient;

public interface IRepairsClient
{
    Task<IReadOnlyList<RepairIssue>> ListAsync(CancellationToken cancellationToken = default);
    Task<RepairIssue?> GetAsync(string domain, string issueId, CancellationToken cancellationToken = default);
    Task<JsonElement> GetIssueDataAsync(string domain, string issueId, CancellationToken cancellationToken = default);
    Task SetIgnoredAsync(string domain, string issueId, bool ignored, CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> StartFixAsync(string domain, string issueId, CancellationToken cancellationToken = default);
    Task<OptionsFlowStep> SubmitFixAsync(string flowId, object userInput, CancellationToken cancellationToken = default);
    Task AbortFixAsync(string flowId, CancellationToken cancellationToken = default);
}

internal sealed class RepairsClient(IHassWebSocketClient ws, IHassApiClient rest) : IRepairsClient
{
    private const string TranslationCategory = "issues";
    private const string TranslationLanguage = "en";

    public async Task<IReadOnlyList<RepairIssue>> ListAsync(CancellationToken cancellationToken = default)
    {
        var issues = await ws.ListRepairIssuesAsync(cancellationToken);
        return await LocalizeAsync(issues, cancellationToken);
    }

    public async Task<RepairIssue?> GetAsync(string domain, string issueId, CancellationToken cancellationToken = default)
    {
        var issues = await ListAsync(cancellationToken);
        return issues.FirstOrDefault(i => i.Domain == domain && i.IssueId == issueId);
    }

    public Task<JsonElement> GetIssueDataAsync(string domain, string issueId, CancellationToken cancellationToken = default) =>
        ws.GetRepairIssueDataAsync(domain, issueId, cancellationToken);

    public Task SetIgnoredAsync(string domain, string issueId, bool ignored, CancellationToken cancellationToken = default) =>
        ws.SetRepairIssueIgnoredAsync(domain, issueId, ignored, cancellationToken);

    public Task<OptionsFlowStep> StartFixAsync(string domain, string issueId, CancellationToken cancellationToken = default) =>
        rest.StartRepairFlowAsync(domain, issueId, cancellationToken);

    public Task<OptionsFlowStep> SubmitFixAsync(string flowId, object userInput, CancellationToken cancellationToken = default) =>
        rest.ConfigureRepairFlowAsync(flowId, userInput, cancellationToken);

    public Task AbortFixAsync(string flowId, CancellationToken cancellationToken = default) =>
        rest.AbortRepairFlowAsync(flowId, cancellationToken);

    private async Task<IReadOnlyList<RepairIssue>> LocalizeAsync(
        IReadOnlyList<RepairIssue> issues,
        CancellationToken cancellationToken)
    {
        if (issues.Count == 0) return issues;

        var domains = issues.Select(i => i.Domain).Distinct(StringComparer.Ordinal).ToArray();
        IReadOnlyDictionary<string, string> resources;
        try
        {
            resources = await ws.GetTranslationsAsync(
                TranslationLanguage, TranslationCategory, domains, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            resources = new Dictionary<string, string>();
        }

        return issues.Select(i => Localize(i, resources)).ToList();
    }

    private static RepairIssue Localize(RepairIssue issue, IReadOnlyDictionary<string, string> resources)
    {
        var prefix = $"component.{issue.Domain}.{TranslationCategory}.{issue.LocalizationKey}";
        var title = Lookup(resources, $"{prefix}.title", issue.TranslationPlaceholders)
                    ?? $"{issue.Domain}: {issue.LocalizationKey}";
        var description = Lookup(resources, $"{prefix}.description", issue.TranslationPlaceholders);

        return issue with { Title = title, Description = description };
    }

    private static string? Lookup(
        IReadOnlyDictionary<string, string> resources,
        string key,
        JsonElement? placeholders) =>
        resources.TryGetValue(key, out var text) ? ApplyPlaceholders(text, placeholders) : null;

    private static string ApplyPlaceholders(string text, JsonElement? placeholders)
    {
        if (placeholders is not { ValueKind: JsonValueKind.Object } obj) return text;

        foreach (var placeholder in obj.EnumerateObject())
        {
            var value = placeholder.Value.ValueKind == JsonValueKind.String
                ? placeholder.Value.GetString() ?? ""
                : placeholder.Value.ToString();
            text = text.Replace($"{{{placeholder.Name}}}", value, StringComparison.Ordinal);
        }

        return text;
    }
}
