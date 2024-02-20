using GraphManagedIdentityRoleAssigner.GraphApi;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;

namespace GraphManagedIdentityRoleAssigner.Commands;

internal class HelloCommand : AsyncCommand<HelloCommand.Settings>
{
    private readonly IOptions<AzureAdOptions> options;

    public HelloCommand(IOptions<AzureAdOptions> options)
    {
        this.options = options;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var prompt = new SelectionPrompt<MenuItem>()
            .Title("Please choose an option:")
            .AddChoices(new List<MenuItem>()
            {
                new() { Id = 0, Text = "Exit" },
                new() { Id = 1, Text = "Display access token" },
                new() { Id = 2, Text = "List users" },
                new() { Id = 3, Text = "Get service principals" },
            });

        var selected = AnsiConsole.Prompt(prompt);

        switch (selected.Id)
        {
            case 0:
                return 0;
            case 1:

                AnsiConsole.MarkupInterpolated($"ClientId: [bold yellow]{options.Value.ClientId}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupInterpolated($"TenantId: [bold yellow]{options.Value.TenantId}[/]");
                AnsiConsole.WriteLine();

                GraphHelper.InitializeGraphForAppAuthOnly(options.Value);
                var token = await GraphHelper.GetAppOnlyTokenAsync();
                AnsiConsole.Write($"App only token: {token}");
                AnsiConsole.WriteLine();
                break;

            case 2:
                try
                {
                    GraphHelper.InitializeGraphForAppAuthOnly(options.Value);
                    var users = await GraphHelper.GetUsersAsync();

                    if (users?.Value == null)
                    {
                        AnsiConsole.WriteLine("No users found");
                        return 0;
                    }

                    foreach (var user in users.Value)
                    {
                        AnsiConsole.WriteLine($"User: {user.DisplayName} ({user.Mail})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }

                break;

            case 3:
                try
                {
                    GraphHelper.InitializeGraphForAppAuthOnly(options.Value);
                    var response = await GraphHelper.GetServicePrincipal();

                    if (response?.Value?.Count > 0)
                    {
                        var servicePrincipals = response.Value.Select(t => new MenuItem
                        {
                            Text = t.DisplayName ?? string.Empty,
                        });

                        var servicePrincipalPrompt = new SelectionPrompt<ServicePrincipal>()
                            .AddChoices(response.Value);

                        var selectedServicePrincipal = AnsiConsole.Prompt(servicePrincipalPrompt);

                        AnsiConsole.MarkupLineInterpolated($"DisplayName: [bold yellow]{selectedServicePrincipal?.DisplayName}[/]");
                        AnsiConsole.MarkupLineInterpolated($"ObjectId: [bold yellow]{selectedServicePrincipal?.Id}[/]");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }

                break;
        }

        return 0;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-n|--name <NAME>")]
        [Description("The thing to greet")]
        [DefaultValue("World")]
        public string Name { get; set; }
    }

    public record MenuItem
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public override string ToString() => $"{Id} - {Text}";
    }
}
