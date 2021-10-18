using Confirmit.Cdl.Api.Authorization.Accessors;
using Confirmit.Cdl.Api.Database.Model;

namespace Confirmit.Cdl.Api.Authorization.Users
{
    public class Enduser : BaseUser
    {
        public Enduser(int id, int listId, CdlDbContext dbContext)
            : base(id, new[] { Role.Enduser }, null, UserType.Enduser, dbContext)
        {
            OrganizationId = listId;
            DocumentAccessor = new DocumentEnduserAccessor(this,
                DbContext.Documents, DbContext.EnduserPermissions, dbContext.EnduserListPermissions);
        }
    }
}