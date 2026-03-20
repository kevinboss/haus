using System.Text.Json;

namespace Haus.Auth;

public static class TokenStore
{
    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "haus");

    private static readonly string FilePath = Path.Combine(Dir, "tokens.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static bool Exists => File.Exists(FilePath);

    public static async Task<TokenData?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath, cancellationToken);
        return JsonSerializer.Deserialize<TokenData>(json);
    }

    public static async Task SaveAsync(TokenData tokenData, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Dir);
        var json = JsonSerializer.Serialize(tokenData, JsonOptions);
        await File.WriteAllTextAsync(FilePath, json, cancellationToken);

        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(FilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }
}
