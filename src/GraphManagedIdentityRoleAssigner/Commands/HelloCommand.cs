using System.Threading.Tasks;

namespace GraphManagedIdentityRoleAssigner.Commands;

internal class HelloCommand : AsyncCommand<HelloCommand.Settings>
{
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine($"Hello [bold yellow]{settings.Name}[/]!");
        return Task.FromResult(0);
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-n|--name <NAME>")]
        [Description("The thing to greet")]
        [DefaultValue("World")]
        public string Name { get; set; }
    }
}
