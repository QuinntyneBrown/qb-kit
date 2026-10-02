using System.CommandLine;

namespace QbKit.New;

public sealed class NewCommand(WorkspaceGenerator generator)
{
    public Command Create()
    {
        var nameArgument = new Argument<string?>("name") { Description = "Workspace name.", Arity = ArgumentArity.ZeroOrOne };
        var command = new Command("new", "Create an Angular workspace.") { nameArgument };
        command.SetAction(async (result, cancellationToken) =>
        {
            var name = result.GetValue(nameArgument);
            if (string.IsNullOrWhiteSpace(name))
            {
                if (Console.IsInputRedirected)
                {
                    Console.Error.WriteLine("A workspace name is required in a noninteractive terminal.");
                    return 2;
                }

                Console.Write("Workspace name: ");
                name = Console.ReadLine();
            }

            return await generator.GenerateAsync(name, cancellationToken);
        });
        return command;
    }
}
