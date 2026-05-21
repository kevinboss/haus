namespace Haus.HassClient;

public interface ITokenProvider
{
    Task<(string Url, string AccessToken)> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
