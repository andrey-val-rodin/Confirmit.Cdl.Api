using Confirmit.Cdl.Api.Database.Contracts;
using Confirmit.Cdl.Api.Middleware;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization.Accessors
{
    [PublicAPI]
    public abstract class BaseAccessor<T> : IAccessor<T> where T : class, IArchivable
    {
        protected BaseAccessor(IUser user, IQueryable<T> entities)
        {
            User = user;
            Entities = entities;
        }

        protected IUser User { get; set; }
        protected byte ResourceType { get; set; }
        protected IQueryable<T> Entities { get; set; }

        public abstract IQueryable<PermittedResource<T>> GetQuery();
        public abstract Task<Permission> GetPermissionAsync(long resourceId, ResourceStatus status);
        public abstract Task<bool> HasPermissionAsync(long resourceId, Permission permission, ResourceStatus status);

        protected async Task<bool> ResourceExistsAsync(long id, ResourceStatus status)
        {
            return await GetBaseQuery(status).AnyAsync(o => o.Id == id);
        }

        protected async Task CheckResourceExistenceAsync(long id, ResourceStatus status)
        {
            if (!await GetBaseQuery(status).AnyAsync(o => o.Id == id))
                throw new NotFoundException($"Resource {id} not found.");
        }

        protected IQueryable<T> GetBaseQuery(ResourceStatus status)
        {
            return status switch
            {
                ResourceStatus.Exists => Entities.Where(o => o.Deleted == null),
                ResourceStatus.Archived => Entities.Where(o => o.Deleted != null),
                _ => Entities
            };
        }
    }
}