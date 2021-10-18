using Confirmit.Cdl.Api.Database.Contracts;
using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class CompanyPermission : IOrganizationPermission
    {
        public long DocumentId { get; set; }

        public int OrganizationId { get; set; }

        public byte Permission { get; set; }
    }
}
