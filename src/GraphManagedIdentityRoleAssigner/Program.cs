using GraphManagedIdentityRoleAssigner;
using GraphManagedIdentityRoleAssigner.Commands;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", true)
    .AddUserSecrets<Program>()
    .Build();

var serviceCollection = new ServiceCollection();

serviceCollection
    .AddSingleton<IConfiguration>(configuration)
    .AddLogging(config =>
    {
        config.AddConsole();
    })
    .AddOptions()
    .Configure<AzureAdOptions>(options => configuration.GetSection("AzureAd").Bind(options));

var registrar = new TypeRegistrar(serviceCollection);

var app = new CommandApp<HelloCommand>(registrar);

app.Configure(config =>
{
    config.AddCommand<HelloCommand>("Hello")
          .WithDescription("Say hello to anyone")
          .WithExample(new string[] { "hello", "--name", "World" });
});

AnsiConsole.WriteLine("GraphManagedIdentityRoleAssigner");

return await app.RunAsync(args);
