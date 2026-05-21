namespace Haus.Hass;

public interface ITokenProvider
{
    Task<(string Url, string AccessToken)> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
