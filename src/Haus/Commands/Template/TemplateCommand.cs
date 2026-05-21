using System.ComponentModel;
using Haus.Auth;
using Haus.Commands;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Template;

public sealed class TemplateCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<TemplateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "[template]")]
        [Description("Jinja2 template to render. Omit to read from --from-file or stdin.")]
        public string? Template { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read template from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            Template is not null && FromFile is not null
                ? ValidationResult.Error("Positional template and --from-file are mutually exclusive.")
                : ValidationResult.Success();
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var template = TextInput.Resolve(settings.Template, settings.FromFile);
        if (string.IsNullOrWhiteSpace(template))
        {
            OutputHelper.WriteError(settings, "No template provided. Pass it as an argument or use --from-file <PATH> (--from-file=- for stdin).");
            return 1;
        }

        var rendered = await api.PostAsync<string>("/api/template", new { template }, cancellationToken);

        OutputHelper.WriteResult(settings, new { template, rendered },
            humanOutput: () => AnsiConsole.WriteLine(rendered),
            porcelainOutput: () => Console.WriteLine(rendered));

        return 0;
    }
}
