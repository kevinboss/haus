using System.Globalization;

namespace Haus.Commands;

public static class DurationParser
{
    public static TimeSpan Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new FormatException("Duration cannot be empty.");

        var trimmed = input.Trim();
        var unit = trimmed[^1];
        if (!char.IsLetter(unit))
            throw new FormatException($"Duration '{input}' must end with s/m/h/d (e.g. 30m, 2h, 1d).");

        if (!double.TryParse(trimmed[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value < 0)
            throw new FormatException($"Duration '{input}' has an invalid number.");

        return char.ToLowerInvariant(unit) switch
        {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            _ => throw new FormatException($"Unknown duration unit '{unit}'. Use s/m/h/d.")
        };
    }
}
