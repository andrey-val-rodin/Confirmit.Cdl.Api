using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.Tools.Validators;
using Confirmit.Cdl.Api.ViewModel;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = Confirmit.Cdl.Api.Accounts.User;

namespace Confirmit.Cdl.Api.Services
{
    [UsedImplicitly]
    public class UserPermissionService : DocumentPermissionService<UserPermission>
    {
        public UserPermissionService(CdlDbContext dbContext, IHttpContextAccessor httpContext,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader)
            : base(dbContext, httpContext.HttpContext.User, mapper, factory, accountLoader, hubPermissionReader,
                surveyPermissionReader)
        {
        }

        public async Task ValidatePermissionsAsync(IList<UserPermissionDto> permissions)
        {
            if (permissions == null || permissions.Count == 0)
                throw new BadRequestException("Empty permission list.");

            var permissionsById = permissions.Where(p => p.Id > 0).ToList();
            var permissionsByKey = permissions.Where(p => !string.IsNullOrEmpty(p.UserKey)).ToList();
            if (permissionsById.Count + permissionsByKey.Count < permissions.Count)
                throw new BadRequestException("Neither Id nor userKey is specified.");

            var users = new List<User>();

            // Proceed permissions by Id
            var ids = permissionsById.Select(p => p.Id).ToList();
            if (ids.Distinct().Count() < permissionsById.Count)
                throw new BadRequestException("Duplicate entry in permission list.");

            var validator = new UsersValidator(AccountLoader);
            await validator.ValidateAsync(ids);
            if (validator.WrongIds.Any())
                throw new NotFoundException($"User {validator.WrongIds.FirstOrDefault()} not found.");

            users.AddRange(validator.ValidUsers);

            // Proceed permissions by key
            foreach (var perm in permissionsByKey)
            {
                var user = await AccountLoader.GetUserAsync(perm.UserKey);
                if (user == null)
                    throw new NotFoundException("User not found.");

                if (users.Any(u => u.UserId == user.UserId))
                    throw new BadRequestException("Duplicate entry in permission list.");

                perm.Id = user.UserId;
                users.Add(user);
            }

            await RefreshUsersAsync(users);
        }

        public async Task ValidatePermissionCompanyAsync(int companyId)
        {
            var company = await AccountLoader.GetCompanyAsync(companyId);
            if (company == null)
                throw new NotFoundException($"Company {companyId} not found.");

            await DbRefresher.RefreshCompanyAsync(company);
        }

        private async Task RefreshUsersAsync(IEnumerable<User> users)
        {
            users = users.ToList();
            var companies = users.Select(u => new Accounts.Company { CompanyId = u.CompanyId, Name = u.CompanyName })
                .Distinct().ToArray();

            // Save companies before users due to foreign constraint
            foreach (var company in companies)
            {
                await DbRefresher.RefreshCompanyAsync(company);
            }

            foreach (var user in users)
            {
                await DbRefresher.RefreshUserAsync(user);
            }
        }

        public IQueryable<UserPermissionFullDto> GetPermissions(long documentId)
        {
            var permissions =
                from perm in DbContext.UserPermissions
                where perm.DocumentId == documentId
                select perm;

            var permissionsAndUsers =
                from perm in permissions
                join user in DbContext.Users on perm.UserId equals user.Id into users
                from user in users.DefaultIfEmpty()
                select
                    new UserPermissionFullDto
                    {
                        Id = perm.UserId,
                        Name = user.Name,
                        FullName = user.FirstName + " " + user.LastName,
                        CompanyId = user.OrganizationId,
                        CompanyName = null,
                        Permission = (Permission) perm.Permission
                    };

            return
                from perm in permissionsAndUsers
                join organization in DbContext.Companies on perm.CompanyId equals organization.Id into
                    organizations
                from organization in organizations.DefaultIfEmpty()
                select
                    new UserPermissionFullDto
                    {
                        Id = perm.Id,
                        Name = perm.Name,
                        FullName = perm.FullName,
                        CompanyId = perm.CompanyId,
                        CompanyName = organization.Name,
                        Permission = perm.Permission
                    };
        }

        public async Task SetUserPermissionsAsync(long documentId, IEnumerable<UserPermissionDto> permissions)
        {
            foreach (var perm in permissions)
            {
                if (perm.Permission == Permission.None)
                    await DeleteUserPermissionFromModelAsync(documentId, perm.Id);
                else
                {
                    if (await FindUserPermissionAsync(documentId, perm.Id) is UserPermission entity)
                        entity.Permission = (byte) perm.Permission;
                    else
                        DbContext.UserPermissions.Add(
                            new UserPermission
                            {
                                DocumentId = documentId,
                                UserId = perm.Id,
                                Permission = (byte) perm.Permission
                            });
                }
            }

            await DbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteUserPermissionFromModelAsync(long documentId, int userId)
        {
            if (await FindUserPermissionAsync(documentId, userId) is UserPermission entity)
            {
                DbContext.UserPermissions.Remove(entity);
                return true;
            }

            return false;
        }

        public async Task SetCompanyPermissionAsync(long documentId, int companyId, Permission permission)
        {
            if (permission == Permission.None)
            {
                await DeleteCompanyPermissionFromModelAsync(documentId, companyId);
            }
            else
            {
                if (await FindOrganizationPermissionAsync(documentId, companyId) is CompanyPermission entity)
                    entity.Permission = (byte) permission;
                else
                    DbContext.CompanyPermissions.Add(
                        new CompanyPermission
                        {
                            DocumentId = documentId,
                            OrganizationId = companyId,
                            Permission = (byte) permission
                        });
            }

            await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteIndividualUserPermissionsAsync(long documentId, int enduserListId)
        {
            return await new QueryProvider().DeleteIndividualUserPermissionsAsync(documentId, enduserListId);
        }

        private async Task<bool> DeleteCompanyPermissionFromModelAsync(long documentId, int companyId)
        {
            if (!(await FindOrganizationPermissionAsync(documentId, companyId) is CompanyPermission entity))
                return false;

            DbContext.CompanyPermissions.Remove(entity);
            return true;
        }

        public async Task<bool> DeleteUserPermissionAsync(long documentId, int userId)
        {
            if (!await DeleteUserPermissionFromModelAsync(documentId, userId))
                return false;

            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCompanyPermissionAsync(long documentId, int companyId)
        {
            if (!await DeleteCompanyPermissionFromModelAsync(documentId, companyId))
                return false;

            await DbContext.SaveChangesAsync();
            return true;
        }
    }
}
