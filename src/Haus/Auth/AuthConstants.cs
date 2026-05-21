namespace Haus.Auth;

public static class AuthConstants
{
    private const int CallbackPort = 18123;
    public static readonly string RedirectUri = $"http://localhost:{CallbackPort}/";
    public static readonly string ClientId = RedirectUri;
}
