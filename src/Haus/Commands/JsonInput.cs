using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands;

internal static class JsonInput
{
    public static ValidationResult ValidateRequired(string? data, string? fromFile)
    {
        if (data is null && fromFile is null)
            return ValidationResult.Error("Provide --data <JSON> or --from-file <PATH> (use --from-file=- for stdin).");
        return ValidateOptional(data, fromFile);
    }

    public static ValidationResult ValidateOptional(string? data, string? fromFile)
    {
        if (data is not null && fromFile is not null)
            return ValidationResult.Error("--data and --from-file are mutually exclusive.");

        if (data is null) return ValidationResult.Success();

        try { JsonDocument.Parse(data); return ValidationResult.Success(); }
        catch (JsonException) { return ValidationResult.Error("--data must be valid JSON."); }
    }

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
