using Confirmit.Cdl.Api.Database.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Confirmit.Cdl.Api.Authorization.Users
{
    public class BaseUser : ICustomer
    {
        protected readonly CdlDbContext DbContext;

        protected BaseUser(int id, Role[] roles, string[] scopes, UserType userType, CdlDbContext dbContext)
        {
            Id = id;
            Roles = new ReadOnlyCollection<Role>(roles);
            Scopes = new ReadOnlyCollection<string>(scopes ?? Enumerable.Empty<string>().ToArray());
            UserType = userType;
            DbContext = dbContext;
        }

        public int Id { get; }
        public int OrganizationId { get; protected set; }
        public UserType UserType { get; }
        public ReadOnlyCollection<Role> Roles { get; }
        public ReadOnlyCollection<string> Scopes { get; }
        public IAccessor<Document> DocumentAccessor { get; protected set; }

        public bool IsInRole(Role role) => Roles.Contains(role);
    }
}