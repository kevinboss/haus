using System.Text.Json;
using Haus.Hass;
using Spectre.Console;

namespace Haus.Output;

public interface IOutputSettings
{
    bool Json { get; }
    bool Porcelain { get; }
}

public static class OutputHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(HassJsonOptions.Default) { WriteIndented = true };

    public static void WriteResult<T>(IOutputSettings settings, T data, Action humanOutput, Action porcelainOutput)
    {
        if (settings.Json)
            Console.WriteLine(JsonSerializer.Serialize(data, JsonOptions));
        else if (settings.Porcelain)
            porcelainOutput();
        else
            humanOutput();
    }

    public static void WriteColumns(string[] headers, IEnumerable<string[]> rows)
    {
        var allRows = rows.ToList();
        Console.WriteLine(string.Join('\t', headers));
        foreach (var row in allRows)
            Console.WriteLine(string.Join('\t', row));
    }

    public static void WriteKeyValue(string key, string value)
    {
        Console.WriteLine($"{key}\t{value}");
    }

    public static void WriteError(IOutputSettings settings, Exception ex)
    {
        WriteError(settings, ex.Message);
    }

    public static void WriteError(IOutputSettings settings, string message)
    {
        if (settings.Json)
            Console.Error.WriteLine(JsonSerializer.Serialize(new { error = message }, JsonOptions));
        else
            AnsiConsole.Console.MarkupLine($"[red]Error:[/] {message.EscapeMarkup()}");
    }
}
