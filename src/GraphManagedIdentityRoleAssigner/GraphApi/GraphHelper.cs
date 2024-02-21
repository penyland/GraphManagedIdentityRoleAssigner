using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace GraphManagedIdentityRoleAssigner.GraphApi;

internal class GraphHelper
{
    private static ClientSecretCredential clientSecretCredential;

    public static GraphServiceClient GraphClient { get; private set; }

    public static void InitializeGraphForAppAuthOnly(AzureAdOptions azureAdOptions)
    {
        clientSecretCredential = new ClientSecretCredential(azureAdOptions.TenantId, azureAdOptions.ClientId, azureAdOptions.ClientSecret);

        GraphClient = new GraphServiceClient(
            clientSecretCredential,
            [
                "https://graph.microsoft.com/.default"
            ]);
    }

    internal static async Task<string> GetAppOnlyTokenAsync()
    {
        var context = new TokenRequestContext(
                       [
                "https://graph.microsoft.com/.default"
            ]);

        var response = await clientSecretCredential.GetTokenAsync(context);
        return response.Token;
    }

    public static Task<UserCollectionResponse?> GetUsersAsync()
    {
        // Ensure client isn't null
        _ = GraphClient ??
            throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        return GraphClient.Users.GetAsync((config) =>
        {
            // Only request specific properties
            config.QueryParameters.Select = new[] { "displayName", "id", "mail" };
            // Get at most 25 results
            config.QueryParameters.Top = 25;
            // Sort by display name
            config.QueryParameters.Orderby = new[] { "displayName" };
        });
    }

    public static async Task<ServicePrincipalCollectionResponse?> GetServicePrincipal(string displayName = "PeterTest")
    {
        _ = GraphClient ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        var count = await GraphClient.ServicePrincipals.Count.GetAsync((requestConfiguration) =>
        {
            requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
        });

        return await GraphClient.ServicePrincipals.GetAsync(config =>
        {
            config.QueryParameters.Search = $"\"displayName:{displayName}\"";
            config.QueryParameters.Count = true;
            config.Headers.Add("ConsistencyLevel", "eventual");
        });
    }

    // Write a method that gets all applications with a tag
    public static async Task<List<Application>> GetApplicationsWithTagAsync(string tag = "")
    {
        _ = GraphClient ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        var applications = await GraphClient.Applications.GetAsync(config =>
        {
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }
            else
            {
                config.QueryParameters.Filter = $"tags/any(t: t eq '{tag}')";
            }

            config.QueryParameters.Select = ["id, displayName, tags"];
            config.Headers.Add("ConsistencyLevel", "eventual");
        });

        return applications?.Value!;

        //if (applications != null && applications.Value != null)
        //{
        //    foreach (var application in applications.Value)
        //    {
        //        AnsiConsole.MarkupLine($"DisplayName: [bold yellow]{application.DisplayName}[/]");
        //        AnsiConsole.MarkupLine($"Id: [bold yellow]{application.Id}[/]");

        //        foreach (var t in application.Tags!)
        //        {
        //            AnsiConsole.MarkupLine($"  Tag: [bold yellow]{t}[/]");
        //        }
        //    }
        //}
    }

    public static async Task GetApplicationRoles(string displayName = "")
    {
        _ = GraphClient ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        var applications = await GraphClient.Applications.GetAsync();
        var application = applications?.Value!.Where(t => t.DisplayName == displayName).FirstOrDefault();

        if (application != null && application.AppRoles != null)
        {
            AnsiConsole.MarkupLine($"DisplayName: [bold yellow]{application.DisplayName}[/]");
            AnsiConsole.MarkupLine($"Id: [bold yellow]{application.Id}[/]");
            AnsiConsole.MarkupLine($"AppId: [bold yellow]{application.AppId}[/]");
            AnsiConsole.MarkupLine($"AppRoles: [bold yellow]{application.AppRoles.Count}[/]");
            foreach (var role in application.AppRoles)
            {
                AnsiConsole.MarkupLine($"  DisplayName: [bold yellow]{role.Value}[/]");
                AnsiConsole.MarkupLine($"  Id: [bold yellow]{role.Id}[/]");
            }
        }
    }

    // Write a method that gets the id of the application role defined on the app registration
    public static async Task<string?> GetApplicationRoleIdAsync(string appId, string roleName)
    {
        _ = GraphClient ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        var response = await GraphClient.Applications.GetAsync(config =>
        {
            config.QueryParameters.Filter = $"appId eq '{appId}'";
            config.QueryParameters.Select = ["id, appId, displayName, appRoles"];
        });

        var app = response?.Value?.FirstOrDefault();
        if (app != null && app.AppRoles != null)
        {
            var role = app.AppRoles.Where(t => t.Value == roleName).FirstOrDefault();
            return role?.Id.ToString();
        }

        return null;
    }

    // Write a method that gets the unique id (object id) of the enterprise application that the app registration is associated with
    public static async Task<string?> GetEnterpriseApplicationIdAsync(string appId)
    {
        _ = GraphClient ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        var response = await GraphClient.ServicePrincipals.GetAsync(config =>
        {
            config.QueryParameters.Filter = $"appId eq '{appId}'";
            config.QueryParameters.Select = ["id, appId, displayName"];
        });

        return response?.Value?.FirstOrDefault()?.Id;
    }
}
