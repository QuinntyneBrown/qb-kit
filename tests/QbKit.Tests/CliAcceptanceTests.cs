using System.Diagnostics;

namespace QbKit.Tests;

// Acceptance Test
// Traces to: L2-001, L2-002, L2-006
// Description: The packaged command exposes help and refuses unsafe names and destinations.
public sealed class CliAcceptanceTests
{
    [Fact]
    public async Task HelpListsNewCommand()
    {
        var result = await RunAsync("--help");
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("new", result.Output);
    }

    [Fact]
    public async Task VersionMatchesPackage()
    {
        var result = await RunAsync("--version");
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("0.1.0", result.Output);
    }

    [Fact]
    public async Task ExistingDestinationIsUnchanged()
    {
        var parent = Path.Combine(Path.GetTempPath(), "qb-kit-existing-" + Guid.NewGuid().ToString("N"));
        var destination = Path.Combine(parent, "sample-app");
        Directory.CreateDirectory(destination);
        var marker = Path.Combine(destination, "marker.txt");
        File.WriteAllText(marker, "keep me");
        try
        {
            var result = await RunAsync(parent, ["new", "sample-app"]);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Equal("keep me", File.ReadAllText(marker));
        }
        finally
        {
            Directory.Delete(parent, recursive: true);
        }
    }

    [Fact]
    public async Task MissingNameWithoutTerminalFails()
    {
        var result = await RunAsync("new");
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("name", result.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("UPPER")]
    [InlineData("con")]
    public async Task UnsafeNameFails(string name)
    {
        var result = await RunAsync("new", name);
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task NewCreatesVerifiedAngularWorkspace()
    {
        var parent = Path.Combine(Path.GetTempPath(), "qb kit acceptance " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(parent);
        var result = await RunAsync(parent, ["new", "sample-app"]);
        Assert.True(result.ExitCode == 0, result.Output + Environment.NewLine + parent);
        var workspace = Path.Combine(parent, "sample-app");
        Assert.True(File.Exists(Path.Combine(workspace, "projects", "sample-app", "src", "app", "app.scss")));
        Assert.True(File.Exists(Path.Combine(workspace, "jest.config.cjs")));
        Assert.True(File.Exists(Path.Combine(workspace, "eslint.config.js")));
        Assert.True(File.Exists(Path.Combine(workspace, "package-lock.json")));
        File.AppendAllText(Path.Combine(workspace, "README.md"), "  badly formatted  ");
        var formatResult = await RunNpmAsync(workspace, "run", "format:check");
        Assert.NotEqual(0, formatResult.ExitCode);
        Directory.Delete(parent, recursive: true);
    }

    private static Task<(int ExitCode, string Output)> RunAsync(params string[] arguments) =>
        RunAsync(Environment.CurrentDirectory, arguments);

    private static async Task<(int ExitCode, string Output)> RunAsync(string workingDirectory, string[] arguments)
    {
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, "QbKit.dll");
        using var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.ArgumentList.Add(assemblyPath);
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await outputTask + await errorTask);
    }

    private static async Task<(int ExitCode, string Output)> RunNpmAsync(string workingDirectory, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "npm";
        process.StartInfo.WorkingDirectory = workingDirectory;
        if (OperatingSystem.IsWindows())
        {
            process.StartInfo.ArgumentList.Add("/d");
            process.StartInfo.ArgumentList.Add("/s");
            process.StartInfo.ArgumentList.Add("/c");
            process.StartInfo.ArgumentList.Add("npm " + string.Join(' ', arguments));
        }
        else foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await outputTask + await errorTask);
    }
}
