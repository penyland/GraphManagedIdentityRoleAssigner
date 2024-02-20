using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;

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

    //public static async Task AssignRoleToServicePrincipalAsync(string servicePrincipalId, string roleId)
    //{
    //    // Ensure client isn't null
    //    _ = GraphClient ??
    //        throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

    //    var appRoleAssignment = new AppRoleAssignment
    //    {
    //        PrincipalId = servicePrincipalId,
    //        ResourceId = roleId
    //    };

    //    await GraphClient.ServicePrincipals[servicePrincipalId].AppRoleAssignments
    //        .Request()
    //        .AddAsync(appRoleAssignment);
    //}

    public static async Task<ServicePrincipalCollectionResponse?> GetServicePrincipal()
    {
        _ = GraphClient ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

        var count = await GraphClient.ServicePrincipals.Count.GetAsync((requestConfiguration) =>
        {
            requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
        });

        return await GraphClient.ServicePrincipals.GetAsync(config =>
        {
            config.QueryParameters.Search = "\"displayName:PeterTest\"";
            config.QueryParameters.Count = true;
            config.Headers.Add("ConsistencyLevel", "eventual");
        });
    }
}
