using System.ComponentModel;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Skill;

public sealed class SkillInstallCommand : AsyncCommand<SkillInstallCommand.Settings>
{
    private const string ResourceName = "Haus.Resources.SKILL.md";

    public sealed class Settings : HausSettings
    {
        [CommandOption("-f|--force")]
        [Description("Overwrite an existing SKILL.md without prompting")]
        public bool Force { get; init; }
    }

    protected override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home))
        {
            OutputHelper.WriteError(settings, "Could not resolve home directory.");
            return Task.FromResult(1);
        }

        var targetDir = Path.Combine(home, ".claude", "skills", "haus");
        var targetFile = Path.Combine(targetDir, "SKILL.md");

        if (File.Exists(targetFile) && !settings.Force)
        {
            OutputHelper.WriteError(settings, $"{targetFile} already exists. Use --force to overwrite.");
            return Task.FromResult(1);
        }

        using var stream = typeof(SkillInstallCommand).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

        Directory.CreateDirectory(targetDir);
        using (var output = File.Create(targetFile))
            stream.CopyTo(output);

        OutputHelper.WriteResult(settings, new { path = targetFile },
            humanOutput: () => AnsiConsole.MarkupLine($"[green]Installed[/] skill to [bold]{targetFile.EscapeMarkup()}[/]"),
            porcelainOutput: () => Console.WriteLine(targetFile));

        return Task.FromResult(0);
    }
}
