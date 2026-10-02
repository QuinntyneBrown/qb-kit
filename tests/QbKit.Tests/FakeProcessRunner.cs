using System.ComponentModel;
using QbKit.Processes;

namespace QbKit.Tests;

public sealed class FakeProcessRunner : IProcessRunner
{
    public string NodeVersion { get; set; } = "v24.18.0";
    public bool MissingNpm { get; set; }
    public bool CancelOnNode { get; set; }

    public Task<int> RunAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Generation should not start for this test.");

    public Task<string> ReadAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        if (CancelOnNode) throw new OperationCanceledException();
        if (fileName == "node") return Task.FromResult(NodeVersion);
        if (MissingNpm) throw new Win32Exception("npm is missing");
        return Task.FromResult("11.16.0");
    }
}
