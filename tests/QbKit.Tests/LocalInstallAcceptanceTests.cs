using System.Diagnostics;
using System.Xml.Linq;

namespace QbKit.Tests;

public sealed class LocalInstallAcceptanceTests
{
    [Theory]
    [InlineData("install-cli.ps1")]
    [InlineData("install-cli.bat")]
    [InlineData("install-cli.sh")]
    public async Task ScriptInstallsAndReinstallsCurrentBuild(string scriptName)
    {
        if (!OperatingSystem.IsWindows() && scriptName != "install-cli.sh") return;
        if (OperatingSystem.IsWindows() && scriptName == "install-cli.sh" &&
            !File.Exists(@"C:\Program Files\Git\bin\bash.exe")) return;

        var repository = FindRepository();
        var project = XDocument.Load(Path.Combine(repository, "src", "QbKit", "QbKit.csproj"));
        var expectedVersion = project.Descendants("Version").Single().Value;
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "qb kit install " + Guid.NewGuid().ToString("N"));
        var toolPath = Path.Combine(temporaryDirectory, "tools");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var result = await RunScriptAsync(Path.Combine(repository, "eng", "scripts", scriptName), temporaryDirectory, toolPath);
                Assert.True(result.ExitCode == 0, result.Output);

                var executable = Path.Combine(toolPath, OperatingSystem.IsWindows() ? "qb-kit.exe" : "qb-kit");
                Assert.True(File.Exists(executable), $"Missing installed command: {executable}");
                var version = await RunAsync(executable, temporaryDirectory);
                Assert.Equal(0, version.ExitCode);
                Assert.Equal(expectedVersion, version.Output.Trim().Split('+')[0]);

            }
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static string FindRepository()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "QbKit.slnx"))) return directory.FullName;
        throw new DirectoryNotFoundException("Repository root not found.");
    }

    private static async Task<(int ExitCode, string Output)> RunScriptAsync(string script, string workingDirectory, string toolPath)
    {
        using var process = new Process();
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.Environment["QBKIT_TOOL_PATH"] = script.EndsWith(".sh", StringComparison.Ordinal) && OperatingSystem.IsWindows()
            ? "/" + char.ToLowerInvariant(toolPath[0]) + toolPath[2..].Replace('\\', '/')
            : toolPath;
        if (script.EndsWith(".ps1", StringComparison.Ordinal))
        {
            process.StartInfo.FileName = "pwsh";
            foreach (var argument in new[] { "-NoProfile", "-File", script }) process.StartInfo.ArgumentList.Add(argument);
        }
        else if (script.EndsWith(".bat", StringComparison.Ordinal))
        {
            process.StartInfo.FileName = "cmd.exe";
            foreach (var argument in new[] { "/d", "/s", "/c", script }) process.StartInfo.ArgumentList.Add(argument);
        }
        else
        {
            process.StartInfo.FileName = OperatingSystem.IsWindows() ? @"C:\Program Files\Git\bin\bash.exe" : "sh";
            process.StartInfo.ArgumentList.Add(script);
        }
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await outputTask + await errorTask);
    }

    private static async Task<(int ExitCode, string Output)> RunAsync(string executable, string workingDirectory)
    {
        using var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.ArgumentList.Add("--version");
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await outputTask + await errorTask);
    }
}
