using System.Text.Json;

namespace Haus.HassClient;

public interface IBackupClient
{
    Task<IReadOnlyList<BackupInfo>> ListAsync(CancellationToken cancellationToken = default);
    Task<JsonElement> GetAsync(string backupId, CancellationToken cancellationToken = default);
    Task<BackupGenerateResult> CreateAsync(string? name, bool full, CancellationToken cancellationToken = default);
    Task DeleteAsync(string backupId, CancellationToken cancellationToken = default);
}

internal sealed class BackupClient(IHassWebSocketClient ws) : IBackupClient
{
    public Task<IReadOnlyList<BackupInfo>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListBackupsAsync(cancellationToken);

    public Task<JsonElement> GetAsync(string backupId, CancellationToken cancellationToken = default) =>
        ws.GetBackupAsync(backupId, cancellationToken);

    // Write to every configured storage agent (matches the UI's default behaviour).
    public async Task<BackupGenerateResult> CreateAsync(string? name, bool full, CancellationToken cancellationToken = default)
    {
        var agents = await ws.ListBackupAgentsAsync(cancellationToken);
        var agentIds = agents.Select(a => a.AgentId).ToList();
        if (agentIds.Count == 0)
            throw new InvalidOperationException("No backup storage agents are available.");
        return await ws.GenerateBackupAsync(agentIds, name, full, cancellationToken);
    }

    public Task DeleteAsync(string backupId, CancellationToken cancellationToken = default) =>
        ws.DeleteBackupAsync(backupId, cancellationToken);
}
