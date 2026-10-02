using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QbKit.New;

namespace QbKit.Tests;

// Acceptance Test
// Traces to: L2-007
// Description: Missing prerequisites and cancellation stop before creating a workspace.
public sealed class PrerequisiteAcceptanceTests
{
    [Fact]
    public async Task UnsupportedNodeDoesNotCreateWorkspace()
    {
        var runner = new FakeProcessRunner { NodeVersion = "v20.0.0" };
        var name = "unsupported-node-" + Guid.NewGuid().ToString("N");
        var result = await CreateGenerator(runner).GenerateAsync(name, CancellationToken.None);
        Assert.NotEqual(0, result);
        Assert.False(Path.Exists(Path.Combine(Environment.CurrentDirectory, name)));
    }

    [Fact]
    public async Task MissingNpmDoesNotCreateWorkspace()
    {
        var runner = new FakeProcessRunner { MissingNpm = true };
        var name = "missing-npm-" + Guid.NewGuid().ToString("N");
        var result = await CreateGenerator(runner).GenerateAsync(name, CancellationToken.None);
        Assert.NotEqual(0, result);
        Assert.False(Path.Exists(Path.Combine(Environment.CurrentDirectory, name)));
    }

    [Fact]
    public async Task CancellationReturnsNonzeroWithoutCreatingWorkspace()
    {
        var runner = new FakeProcessRunner { CancelOnNode = true };
        var name = "cancelled-" + Guid.NewGuid().ToString("N");
        var result = await CreateGenerator(runner).GenerateAsync(name, CancellationToken.None);
        Assert.Equal(130, result);
        Assert.False(Path.Exists(Path.Combine(Environment.CurrentDirectory, name)));
    }

    private static WorkspaceGenerator CreateGenerator(FakeProcessRunner runner)
    {
        var options = Options.Create(new ToolingOptions { Angular = "22.2.1", AngularEslint = "22.5.0", Jest = "30.5.2", JestPreset = "17.0.1" });
        return new WorkspaceGenerator(runner, new WorkspaceConfigurator(options), options, NullLogger<WorkspaceGenerator>.Instance);
    }
}
