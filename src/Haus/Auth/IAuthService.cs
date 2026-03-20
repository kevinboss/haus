namespace Haus.Auth;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    Task LoginAsync(string url, CancellationToken cancellationToken = default);
    Task<(string Url, string AccessToken)> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
