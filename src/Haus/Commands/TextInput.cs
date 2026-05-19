namespace Haus.Commands;

internal static class TextInput
{
    public static string? Resolve(string? data, string? fromFile)
    {
        if (data is not null) return data;
        if (fromFile is null) return null;
        if (fromFile == "-") return Console.In.ReadToEnd();

        if (!File.Exists(fromFile))
            throw new FileNotFoundException($"--from-file path not found: {fromFile}", fromFile);

        return File.ReadAllText(fromFile);
    }
}
