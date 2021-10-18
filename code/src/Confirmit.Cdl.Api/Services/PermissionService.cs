using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Contracts;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Services
{
    public class DocumentPermissionService<T> : BaseService where T : class, IUserPermission
    {
        private readonly Type _type = typeof(T);
        private readonly IQueryable<Database.Contracts.IUser> _users;
        private readonly IQueryable<IOrganization> _organizations;
        private readonly IQueryable<IUserPermission> _userPermissions;
        private readonly IQueryable<IOrganizationPermission> _organizationPermissions;

        protected DocumentPermissionService(CdlDbContext dbContext, ClaimsPrincipal principal,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader)
            : base(dbContext, principal, mapper, factory, accountLoader, hubPermissionReader, surveyPermissionReader)
        {
            if (_type == typeof(UserPermission))
            {
                _users = DbContext.Users;
                _organizations = DbContext.Companies;
                _userPermissions = DbContext.UserPermissions;
                _organizationPermissions = DbContext.CompanyPermissions;
            }
            else
            {
                if (_type != typeof(EnduserPermission))
                    throw new InvalidOperationException("T type must be UserPermission or EnduserPermission");

                _users = DbContext.Endusers;
                _organizations = DbContext.EnduserLists;
                _userPermissions = DbContext.EnduserPermissions;
                _organizationPermissions = DbContext.EnduserListPermissions;
            }
        }

        public IQueryable<OrganizationPermissionDto> GetOrganizationPermissions(long documentId)
        {
            return from perm in _organizationPermissions
                where perm.DocumentId == documentId
                from organization in _organizations
                where (int) organization.Id == perm.OrganizationId
                select new OrganizationPermissionDto
                {
                    Id = perm.OrganizationId,
                    Name = organization.Name,
                    Permission = (Permission) perm.Permission
                };
        }

        public IQueryable<OrganizationDto> GetUserPermissionsOrganizations(long documentId)
        {
            var organizationsByUsers =
                from perm in _userPermissions
                where perm.DocumentId == documentId
                from user in _users
                where (int) user.Id == perm.UserId
                group user by user.OrganizationId
                into grouping
                select grouping.Key;

            return
                from dist in organizationsByUsers
                from organization in _organizations
                where (int) organization.Id == dist
                select new OrganizationDto { Id = (int) organization.Id, Name = organization.Name };
        }

        public IQueryable<OrganizationDto> GetAllOrganizations(long documentId)
        {
            var organizationsByUsers =
                from perm in _userPermissions
                where perm.DocumentId == documentId
                from user in _users
                where (int) user.Id == perm.UserId
                group user by user.OrganizationId
                into grouping
                select grouping.Key;
            var wholeOrganizations =
                from perm in _organizationPermissions
                where perm.DocumentId == documentId
                select perm.OrganizationId;
            var allOrganizations = organizationsByUsers.Union(wholeOrganizations);

            return
                from dist in allOrganizations
                from organization in _organizations
                where (int) organization.Id == dist
                select new OrganizationDto { Id = (int) organization.Id, Name = organization.Name };
        }

        protected async Task<IUserPermission> FindUserPermissionAsync(long documentId, int userId)
        {
            return await _userPermissions.FirstOrDefaultAsync(
                p => p.DocumentId == documentId && p.UserId == userId);
        }

        protected async Task<IOrganizationPermission> FindOrganizationPermissionAsync(
            long documentId, int organizationId)
        {
            return await _organizationPermissions.FirstOrDefaultAsync(
                p => p.DocumentId == documentId && p.OrganizationId == organizationId);
        }
    }
}
