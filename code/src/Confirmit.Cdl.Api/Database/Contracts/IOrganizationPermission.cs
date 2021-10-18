using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Contracts
{
    [PublicAPI]
    public interface IOrganizationPermission
    {
        long DocumentId { get; }
        int OrganizationId { get; }
        byte Permission { get; }
    }
}
