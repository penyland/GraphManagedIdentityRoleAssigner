using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphManagedIdentityRoleAssigner;

internal class AzureAdOptions
{
    public string? ClientId { get; set; }

    public string? TenantId { get; set; }

    public string? ClientSecret { get; set; }

    public string? Authority { get; set; }

    public string? GraphResourceId { get; set; }

    public string? GraphResourceIdSecret { get; }
}
