using Confirmit.Cdl.Api.Database.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization.Accessors
{
    // Accessor with full unconditional access to all resources.
    // Can be used by administrator
    public class AbsoluteAccessor<T> : BaseAccessor<T> where T : class, IArchivable
    {
        public AbsoluteAccessor(ICustomer user, IQueryable<T> entities)
            : base(user, entities)
        {
        }

        public override IQueryable<PermittedResource<T>> GetQuery()
        {
            return
                from resource in Entities
                select new PermittedResource<T>
                {
                    Resource = resource,
                    Permission = Permission.Manage
                };
        }

        public override async Task<Permission> GetPermissionAsync(long resourceId, ResourceStatus status)
        {
            await CheckResourceExistenceAsync(resourceId, status);
            return Permission.Manage;
        }

        public override async Task<bool> HasPermissionAsync(long resourceId, Permission permission, ResourceStatus status)
        {
            if (permission == Permission.None)
                throw new ArgumentException(nameof(permission));

            await CheckResourceExistenceAsync(resourceId, status);
            return true;
        }
    }
}