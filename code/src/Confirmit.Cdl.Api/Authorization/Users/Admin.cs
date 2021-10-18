using Confirmit.Cdl.Api.Authorization.Accessors;
using Confirmit.Cdl.Api.Database.Model;

namespace Confirmit.Cdl.Api.Authorization.Users
{
    public class Admin : BaseUser
    {
        public Admin(int id, int companyId, CdlDbContext dbContext)
            : base(id, new[] { Role.Administrator }, null, UserType.User, dbContext)
        {
            OrganizationId = companyId;
            DocumentAccessor = new AbsoluteAccessor<Document>(this, DbContext.Documents);
        }
    }
}