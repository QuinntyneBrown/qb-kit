using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QbKit.New;
using QbKit.Processes;
using System.CommandLine;

var verbose = args.Contains("--verbose", StringComparer.Ordinal);
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = [] });
builder.Logging.SetMinimumLevel(verbose ? LogLevel.Information : LogLevel.Warning);
builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
builder.Services.AddSingleton<WorkspaceGenerator>();
builder.Services.AddSingleton<WorkspaceConfigurator>();
builder.Services.AddSingleton<CounterComponentScaffolder>();
builder.Services.AddSingleton<NewCommand>();
builder.Services.AddOptions<ToolingOptions>().Configure(options =>
{
    options.Angular = "22.2.1";
    options.AngularEslint = "22.5.0";
    options.Jest = "30.5.2";
    options.JestPreset = "17.0.1";
}).Validate(options => options.Angular.Length > 0 && options.AngularEslint.Length > 0 && options.Jest.Length > 0 && options.JestPreset.Length > 0).ValidateOnStart();
using var host = builder.Build();

var root = new RootCommand("Create and manage qb-kit workspaces.");
var verboseOption = new Option<bool>("--verbose") { Description = "Show step details." };
root.Options.Add(verboseOption);
root.Subcommands.Add(host.Services.GetRequiredService<NewCommand>().Create());
return await root.Parse(args).InvokeAsync();
