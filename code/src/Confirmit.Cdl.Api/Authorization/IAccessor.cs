using Confirmit.Cdl.Api.Database.Contracts;
using JetBrains.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization
{
    [PublicAPI]
    public interface IAccessor<T> where T : class, IArchivable
    {
        IQueryable<PermittedResource<T>> GetQuery();
        Task<Permission> GetPermissionAsync(long resourceId, ResourceStatus status);
        Task<bool> HasPermissionAsync(long resourceId, Permission permission, ResourceStatus status);
    }
}