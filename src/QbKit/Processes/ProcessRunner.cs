using System.Diagnostics;

namespace QbKit.Processes;

public sealed class ProcessRunner : IProcessRunner
{
    private static ProcessStartInfo CreateStartInfo(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo { WorkingDirectory = workingDirectory };
        if (OperatingSystem.IsWindows() && fileName == "npm")
        {
            if (arguments.Any(argument => !System.Text.RegularExpressions.Regex.IsMatch(argument, "^[a-zA-Z0-9@/._:=+\\-]+$")))
                throw new ArgumentException("Unsupported npm argument.");
            startInfo.FileName = "cmd.exe";
            startInfo.ArgumentList.Add("/d");
            startInfo.ArgumentList.Add("/s");
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add("npm " + string.Join(' ', arguments));
        }
        else
        {
            startInfo.FileName = fileName;
            foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
        }
        return startInfo;
    }

    public async Task<int> RunAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = CreateStartInfo(fileName, arguments, workingDirectory);
        process.Start();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            throw;
        }
    }

    public async Task<string> ReadAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = CreateStartInfo(fileName, arguments, workingDirectory);
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        try
        {
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var output = await outputTask;
            var error = await errorTask;
            if (process.ExitCode != 0) throw new InvalidOperationException(error.Length > 0 ? error : output);
            return output.Trim();
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            throw;
        }
    }
}
