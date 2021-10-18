using Confirmit.Cdl.Api.Database.Contracts;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization.Accessors
{
    public class DocumentAccessor : BaseAccessor<Document>
    {
        private readonly IEnumerable<int> _companiesWithAdminAccess;
        private readonly IQueryable<IUserPermission> _userPermissions;
        private readonly IQueryable<IOrganizationPermission> _organizationPermissions;

        public DocumentAccessor(
            ICustomer user,
            IEnumerable<int> companiesWithAdminAccess,
            IQueryable<Document> entities,
            IQueryable<IUserPermission> userPermissions,
            IQueryable<IOrganizationPermission> organizationPermissions)
            : base(user, entities)
        {
            _companiesWithAdminAccess = companiesWithAdminAccess ?? Enumerable.Empty<int>();
            _userPermissions = userPermissions;
            _organizationPermissions = organizationPermissions;
        }

        public override IQueryable<PermittedResource<Document>> GetQuery()
        {
            return
                from obj in GetQueryWithExplicitPermissions()
                // implicit rule 1: permission Manage to documents from own companies
                let
                    ownCompany = _companiesWithAdminAccess.Contains(obj.Resource.CompanyId)
                // implicit rule 2: scope 'api.surveyrights' => permission Manage to documents with type DataTemplate
                let
                    scopeSurveyrights = User.Scopes.Contains("api.surveyrights") &&
                                        obj.Resource.Type == (byte) DocumentType.DataTemplate
                // implicit rule 3: scope 'api.cdl.read' => permission View to all documents
                let
                    scopeApiCdlRead = User.Scopes.Contains("api.cdl.read")
                let
                    permission = ownCompany
                        ? Permission.Manage
                        : scopeSurveyrights
                            ? Permission.Manage
                            : scopeApiCdlRead && obj.Permission <= Permission.View
                                ? Permission.View
                                : obj.Permission
                where
                    permission > Permission.None
                select new PermittedResource<Document>
                {
                    Resource = obj.Resource,
                    Permission = permission
                };
        }

        private IQueryable<PermittedResource<Document>> GetQueryWithExplicitPermissions()
        {
            var userPermissions = _userPermissions.Where(p =>
                p.UserId == User.Id);
            var organizationPermissions = _organizationPermissions.Where(p =>
                p.OrganizationId == User.OrganizationId);
            return
                from doc in Entities
                join ups in userPermissions on doc.Id equals ups.DocumentId into ups
                from up in ups.DefaultIfEmpty()
                join ops in organizationPermissions on doc.Id equals ops.DocumentId into ops
                from op in ops.DefaultIfEmpty()
                let
                    u = up == null ? Permission.None : (Permission) up.Permission
                let
                    o = op == null ? Permission.None : (Permission) op.Permission
                select new PermittedResource<Document>
                {
                    Resource = doc,
                    Permission = u > o ? u : o
                };
        }

        public override async Task<Permission> GetPermissionAsync(long documentId, ResourceStatus status)
        {
            await CheckResourceExistenceAsync(documentId, status);

            // explicit permissions
            var permission = await GetUserPermissionAsync(documentId, User.Id, _userPermissions, status);
            if (permission == Permission.Manage)
                return permission;

            permission = Max(permission, await GetOrganizationPermissionAsync(
                documentId, User.OrganizationId, _organizationPermissions, status));
            if (permission == Permission.Manage)
                return permission;

            // implicit rule 1: permission Manage to documents from own companies
            if (await IsOwnCompanyDocumentAsync(documentId, status))
                return Permission.Manage;

            // implicit rule 2: scope 'api.surveyrights' => permission Manage to documents with type DataTemplate
            if (User.Scopes.Contains("api.surveyrights") &&
                await GetDocumentType(documentId, status) == DocumentType.DataTemplate)
                return Permission.Manage;

            // implicit rule 3: scope 'api.cdl.read' => permission View to all documents
            if (User.Scopes.Contains("api.cdl.read"))
                permission = Permission.View;

            return permission;
        }

        private async Task<Permission> GetUserPermissionAsync(
            long documentId, int userId,
            IQueryable<IUserPermission> permissions,
            ResourceStatus status)
        {
            var query =
                from obj in GetBaseQuery(status)
                from p in permissions
                where
                    p.DocumentId == documentId &&
                    p.UserId == userId
                select p.Permission;

            return (Permission) await query.FirstOrDefaultAsync();
        }

        private async Task<Permission> GetOrganizationPermissionAsync(
            long documentId, int organizationId,
            IQueryable<IOrganizationPermission> permissions,
            ResourceStatus status)
        {
            var query =
                from obj in GetBaseQuery(status)
                from p in permissions
                where
                    p.DocumentId == documentId &&
                    p.OrganizationId == organizationId
                select p.Permission;

            return (Permission) await query.FirstOrDefaultAsync();
        }

        private static Permission Max(Permission p1, Permission p2)
        {
            return (Permission) Math.Max((int) p1, (int) p2);
        }

        private async Task<bool> IsOwnCompanyDocumentAsync(long documentId, ResourceStatus status)
        {
            return await GetBaseQuery(status)
                .AnyAsync(o => o.Id == documentId && _companiesWithAdminAccess.Contains(o.CompanyId));
        }

        private async Task<DocumentType> GetDocumentType(long documentId, ResourceStatus status)
        {
            return (DocumentType) await GetBaseQuery(status).Where(d => d.Id == documentId).Select(d => d.Type)
                .FirstOrDefaultAsync();
        }

        public override async Task<bool> HasPermissionAsync(long documentId, Permission permission,
            ResourceStatus status)
        {
            if (permission == Permission.None)
                throw new ArgumentException(nameof(permission));

            return await GetPermissionAsync(documentId, status) >= permission;
        }
    }
}