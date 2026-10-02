using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QbKit.Processes;

namespace QbKit.New;

public sealed class WorkspaceGenerator(IProcessRunner runner, WorkspaceConfigurator configurator, IOptions<ToolingOptions> options, ILogger<WorkspaceGenerator> logger)
{
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "con", "prn", "aux", "nul", "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
        "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
    };

    public async Task<int> GenerateAsync(string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            !System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$") ||
            ReservedNames.Contains(name))
        {
            Console.Error.WriteLine("Workspace name must be lowercase kebab-case and cannot be a reserved system name.");
            return 2;
        }

        var destination = Path.Combine(Environment.CurrentDirectory, name);
        if (Path.Exists(destination))
        {
            Console.Error.WriteLine($"Destination already exists: {destination}");
            return 2;
        }

        var parent = Environment.CurrentDirectory;
        var step = "checking prerequisites";
        try
        {
            var nodeVersion = (await runner.ReadAsync("node", ["--version"], parent, cancellationToken)).TrimStart('v');
            if (!Version.TryParse(nodeVersion, out var parsedNode) ||
                !(parsedNode.Major == 22 && parsedNode >= new Version(22, 22, 3) ||
                  parsedNode.Major == 24 && parsedNode >= new Version(24, 15) ||
                  parsedNode.Major >= 26))
            {
                Console.Error.WriteLine($"Unsupported Node version {nodeVersion}. Angular 22 requires Node 22.22.3+, 24.15+, or 26+.");
                return 2;
            }
            await runner.ReadAsync("npm", ["--version"], parent, cancellationToken);

            step = "creating Angular workspace";
            await RunStepAsync(step, parent, ["exec", "--yes", $"--package=@angular/cli@{options.Value.Angular}", "--", "ng", "new", name,
                "--create-application=false", "--skip-git", "--skip-install", "--defaults", "--new-project-root=projects", "--package-manager=npm", "--interactive=false"], cancellationToken);

            step = "generating Angular application";
            await RunStepAsync(step, destination, ["exec", "--yes", $"--package=@angular/cli@{options.Value.Angular}", "--", "ng", "generate", "application", name,
                "--style=scss", "--routing", "--standalone", "--strict", "--zoneless", "--skip-install", "--interactive=false"], cancellationToken);

            step = "configuring development tools";
            configurator.Configure(destination, name);

            step = "installing npm dependencies";
            await RunStepAsync(step, destination, ["install"], cancellationToken);
            step = "formatting workspace";
            await RunStepAsync(step, destination, ["run", "format"], cancellationToken);
            step = "building application";
            await RunStepAsync(step, destination, ["run", "build"], cancellationToken);
            step = "linting application";
            await RunStepAsync(step, destination, ["run", "lint"], cancellationToken);
            step = "checking formatting";
            await RunStepAsync(step, destination, ["run", "format:check"], cancellationToken);
            step = "running Jest tests";
            await RunStepAsync(step, destination, ["test"], cancellationToken);

            Console.WriteLine($"Workspace ready: {destination}");
            Console.WriteLine($"Next: cd {name} && npm start");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine($"Cancelled during {step}. Partial workspace retained at {destination}.");
            return 130;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Failed while {step}: {exception.Message}");
            if (Directory.Exists(destination)) Console.Error.WriteLine($"Partial workspace retained at {destination}.");
            return 1;
        }
    }

    private async Task RunStepAsync(string step, string directory, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{step}...");
        logger.LogInformation("Running npm {Arguments} in {Directory}", string.Join(' ', arguments), directory);
        var exitCode = await runner.RunAsync("npm", arguments, directory, cancellationToken);
        if (exitCode != 0) throw new InvalidOperationException($"npm exited with code {exitCode}.");
    }
}
