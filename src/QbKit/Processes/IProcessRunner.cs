namespace QbKit.Processes;

public interface IProcessRunner
{
    Task<int> RunAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken);
    Task<string> ReadAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken);
}
