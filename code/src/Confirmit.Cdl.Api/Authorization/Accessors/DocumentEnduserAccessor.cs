using Confirmit.Cdl.Api.Database.Contracts;
using Confirmit.Cdl.Api.Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization.Accessors
{
    public class DocumentEnduserAccessor : DocumentAccessor
    {
        public DocumentEnduserAccessor(
            ICustomer user,
            IQueryable<Document> entities,
            IQueryable<IUserPermission> userPermissions,
            IQueryable<IOrganizationPermission> organizationPermissions)
            : base(user, null, entities, userPermissions, organizationPermissions)
        {
        }

        public override IQueryable<PermittedResource<Document>> GetQuery()
        {
            return base.GetQuery().Where(d => d.Resource.Deleted == null && d.Resource.PublishedRevisionId != null);
        }

        public override async Task<Permission> GetPermissionAsync(long documentId, ResourceStatus status)
        {
            if (!await IsDocumentPublishedAsync(documentId))
                return Permission.None;

            return await base.GetPermissionAsync(documentId, ResourceStatus.Exists);
        }

        private async Task<bool> IsDocumentPublishedAsync(long documentId)
        {
            var publishedRevisionId = await GetBaseQuery(ResourceStatus.Exists).Where(d => d.Id == documentId)
                .Select(d => d.PublishedRevisionId).FirstOrDefaultAsync();
            return publishedRevisionId != null;
        }
    }
}