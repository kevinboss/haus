namespace Haus.Commands.Helper;

internal enum HelperKind
{
    Boolean,
    Text,
    Number,
    Select,
    Datetime,
    Counter,
    Timer
}

internal static class HelperKinds
{
    public static string Domain(this HelperKind kind) => kind switch
    {
        HelperKind.Boolean => "input_boolean",
        HelperKind.Text => "input_text",
        HelperKind.Number => "input_number",
        HelperKind.Select => "input_select",
        HelperKind.Datetime => "input_datetime",
        HelperKind.Counter => "counter",
        HelperKind.Timer => "timer",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    public static HelperKind? FromDomain(string domain) => domain switch
    {
        "input_boolean" => HelperKind.Boolean,
        "input_text" => HelperKind.Text,
        "input_number" => HelperKind.Number,
        "input_select" => HelperKind.Select,
        "input_datetime" => HelperKind.Datetime,
        "counter" => HelperKind.Counter,
        "timer" => HelperKind.Timer,
        _ => null
    };
}
