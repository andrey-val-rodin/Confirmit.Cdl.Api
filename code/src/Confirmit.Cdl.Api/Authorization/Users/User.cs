using Confirmit.Cdl.Api.Authorization.Accessors;
using Confirmit.Cdl.Api.Database.Model;

namespace Confirmit.Cdl.Api.Authorization.Users
{
    public class User : BaseUser
    {
        public User(int id, int companyId, int[] companies, string[] scopes, CdlDbContext dbContext)
            : base(id, new[] { Role.NormalUser }, scopes, UserType.User, dbContext)
        {
            OrganizationId = companyId;
            DocumentAccessor = new DocumentAccessor(this, companies,
                DbContext.Documents, DbContext.UserPermissions, dbContext.CompanyPermissions);
        }
    }
}