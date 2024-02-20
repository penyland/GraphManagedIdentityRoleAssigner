using System;
using GraphManagedIdentityRoleAssigner;
using GraphManagedIdentityRoleAssigner.Commands;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", true)
    .Build();

var serviceCollection = new ServiceCollection();

serviceCollection
    .AddSingleton<IConfiguration>(configuration)
    .AddLogging(config =>
    {
        config.AddConsole();
    });

var registrar = new TypeRegistrar(serviceCollection);

var app = new CommandApp<HelloCommand>(registrar);

app.Configure(config =>
{
    config.AddCommand<HelloCommand>("Hello")
          .WithDescription("Say hello to anyone")
          .WithExample(new string[] { "hello", "--name", "World" });
});

AnsiConsole.Write(
    new FigletText("GraphManagedIdentityRoleAssigner")
        .LeftJustified()
        .Color(Color.Red));

return await app.RunAsync(args);
