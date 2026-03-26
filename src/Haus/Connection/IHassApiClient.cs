namespace Haus.Connection;

public interface IHassApiClient
{
    Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default);
    Task<T> PostAsync<T>(string path, object? data = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
