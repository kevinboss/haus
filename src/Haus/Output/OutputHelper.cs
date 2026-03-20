using System.Text.Json;
using Spectre.Console;

namespace Haus.Output;

public static class OutputHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void WriteResult<T>(bool json, T data, Action humanOutput)
    {
        if (json)
            Console.WriteLine(JsonSerializer.Serialize(data, JsonOptions));
        else
            humanOutput();
    }

    public static void WriteError(bool json, Exception ex)
    {
        WriteError(json, ex.Message);
    }

    public static void WriteError(bool json, string message)
    {
        if (json)
            Console.Error.WriteLine(JsonSerializer.Serialize(new { error = message }, JsonOptions));
        else
            AnsiConsole.Console.MarkupLine($"[red]Error:[/] {message.EscapeMarkup()}");
    }
}
