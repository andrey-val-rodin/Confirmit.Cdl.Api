using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.Tools.Excel;
using Confirmit.Cdl.Api.Tools.Validators;
using Confirmit.Cdl.Api.ViewModel;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Services
{
    [UsedImplicitly]
    public class EnduserPermissionService : DocumentPermissionService<EnduserPermission>
    {
        public EnduserPermissionService(CdlDbContext dbContext, IHttpContextAccessor httpContext,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader)
            : base(dbContext, httpContext.HttpContext.User, mapper, factory, accountLoader, hubPermissionReader,
                surveyPermissionReader)
        {
        }

        public async Task<Stream> GetPermissionsForEnduserListAsync(long documentId, int enduserListId)
        {
            var permissions = await GetEnduserPermissionsAsync(documentId, enduserListId);
            if (permissions == null)
                throw new NotFoundException($"Enduser list {enduserListId} not found.");

            var permissionList = permissions.ToList();
            if (await FindOrganizationPermissionAsync(documentId, enduserListId) != null)
                permissionList.ForEach(p => p.Permission = Permission.View);

            var stream = new Writer(permissionList).Write();
            return stream;
        }

        public async Task<IEnumerable<EnduserPermissionFullDto>> GetEnduserPermissionsAsync(long documentId,
            int enduserListId)
        {
            var enduserList = await AccountLoader.GetEnduserListAsync(enduserListId);
            if (enduserList == null)
                // Current principal has not access to enduser list
                return null;

            var enduserCompanies = await AccountLoader.GetEnduserListCompaniesAsync(enduserListId);
            if (enduserCompanies == null)
                // Something goes wrong
                return null;

            var endusers = await AccountLoader.GetEndusersInListAsync(enduserListId);
            if (endusers == null)
                // Something goes wrong
                return null;

            if (!endusers.Any())
                // No endusers in list
                return Enumerable.Empty<EnduserPermissionFullDto>();

            var ids = endusers.Select(eu => eu.Id);
            var query =
                from perm in DbContext.EnduserPermissions
                where perm.DocumentId == documentId &&
                      ids.Contains(perm.UserId)
                select perm;
            var permissions = await query.ToDictionaryAsync(p => p.UserId);

            return
                from user in endusers
                join company in enduserCompanies on user.CompanyId equals company.Id into companies
                from company in companies.DefaultIfEmpty()
                select new EnduserPermissionFullDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    EnduserListId = user.ListId,
                    EnduserListName = enduserList.Name,
                    EnduserCompanyId = user.CompanyId,
                    EnduserCompanyName = company?.Name,
                    Permission = permissions.ContainsKey(user.Id)
                        ? (Permission) permissions[user.Id].Permission
                        : Permission.None
                };
        }

        public IQueryable<EnduserPermissionFullDto> GetPermissions(long documentId)
        {
            var permissions =
                from perm in DbContext.EnduserPermissions
                where perm.DocumentId == documentId
                select perm;

            return
                from perm in permissions
                join user in DbContext.Endusers on perm.UserId equals user.Id into users
                from user in users.DefaultIfEmpty()
                join company in DbContext.EnduserCompanies on user.CompanyId equals company.Id into companies
                from company in companies.DefaultIfEmpty()
                join organization in DbContext.EnduserLists on user.OrganizationId equals organization.Id into
                    organizations
                from organization in organizations.DefaultIfEmpty()
                select
                    new EnduserPermissionFullDto
                    {
                        Id = perm.UserId,
                        Name = user.Name,
                        FullName = user.FirstName + " " + user.LastName,
                        Email = user.Email,
                        EnduserListId = user.OrganizationId,
                        EnduserListName = organization.Name,
                        EnduserCompanyId = company.Id,
                        EnduserCompanyName = company.Name,
                        Permission = (Permission) perm.Permission
                    };
        }

        public async Task<IList<Accounts.Enduser>> ValidatePermissionsAsync(IList<PermissionDto> permissions)
        {
            if (permissions == null || permissions.Count == 0)
                throw new BadRequestException("Empty permission list.");

            if (permissions.Any(perm => perm.Permission == Permission.Manage))
                throw new BadRequestException("Only permissions None or View can be specified for enduser.");

            var ids = permissions.Select(p => p.Id).ToList();
            if (ids.Distinct().Count() < permissions.Count)
                throw new BadRequestException("Duplicate entry in permission list.");

            var validator = new EndusersValidator(AccountLoader);
            await validator.ValidateAsync(ids);
            if (validator.WrongIds.Any())
                throw new NotFoundException($"Enduser {validator.WrongIds.FirstOrDefault()} not found.");

            if (!validator.ValidUsers.Any())
                return validator.ValidUsers;

            await RefreshEndusersAsync(validator.ValidUsers);

            return validator.ValidUsers;
        }

        [AssertionMethod]
        public async Task ValidatePermissionAsync(
            int enduserListId, Permission permission)
        {
            if (permission == Permission.Manage)
                throw new BadRequestException("Only permission View can be specified for enduser list.");

            var list = await AccountLoader.GetEnduserListAsync(enduserListId);
            if (list == null)
                throw new NotFoundException($"Enduser list {enduserListId} not found.");

            await DbRefresher.RefreshEnduserListAsync(list);
        }

        private async Task RefreshEndusersAsync(IEnumerable<Accounts.Enduser> endusers)
        {
            endusers = endusers.ToList();
            var enduserListIds = endusers.Select(u => u.ListId).Distinct().ToArray();
            var enduserLists = await AccountLoader.GetManyEnduserListsAsync(enduserListIds);
            if (enduserLists == null)
                throw new InvalidOperationException("Attempt to refresh endusers without enduser lists");

            // Save enduser lists before endusers due to foreign constraint
            foreach (var list in enduserLists)
            {
                await DbRefresher.RefreshEnduserListAsync(list);
                // Save enduser list companies before endusers due to foreign constraint
                var companies = await AccountLoader.GetEnduserListCompaniesAsync(list.Id);
                if (companies == null)
                    throw new InvalidOperationException("Attempt to refresh endusers without enduser companies");

                foreach (var company in companies)
                {
                    await DbRefresher.RefreshEnduserCompanyAsync(company);
                }
            }

            foreach (var enduser in endusers)
            {
                await DbRefresher.RefreshEnduserAsync(enduser);
            }
        }

        public async Task SetEnduserPermissionsAsync(long documentId, IEnumerable<PermissionDto> permissions,
            IEnumerable<Accounts.Enduser> endusers)
        {
            var enduserDictionary = endusers.ToDictionary(e => e.Id);
            foreach (var perm in permissions)
            {
                if (!enduserDictionary.ContainsKey(perm.Id))
                    continue;

                if (perm.Permission == Permission.None)
                    await DeleteEnduserPermissionFromModelAsync(documentId, perm.Id);
                else
                {
                    if (await FindUserPermissionAsync(documentId, perm.Id) is EnduserPermission entity)
                        entity.Permission = (byte) perm.Permission;
                    else
                        DbContext.EnduserPermissions.Add(
                            new EnduserPermission
                            {
                                DocumentId = documentId,
                                UserId = perm.Id,
                                Permission = (byte) perm.Permission
                            });
                }
            }

            var enduserListIds = enduserDictionary.Values.Select(e => e.ListId).Distinct().ToList();
            foreach (var enduserListId in enduserListIds)
            {
                await InsertSelectedEnduserListAsync(documentId, enduserListId);
            }

            await DbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteEnduserPermissionFromModelAsync(long documentId, int enduserId)
        {
            if (await FindUserPermissionAsync(documentId, enduserId) is EnduserPermission entity)
            {
                DbContext.EnduserPermissions.Remove(entity);
                return true;
            }

            return false;
        }

        public async Task<ExcelUploadDto> SetEnduserPermissionsAsync(long documentId, HttpRequest request)
        {
            var stream = await GetStream(request);
            var reader = new Reader(AccountLoader);
            var (permissions, endusers, errors) = await reader.Parse(stream);

            if (permissions.Count > 0)
            {
                await RefreshEndusersAsync(endusers);
                await SetEnduserPermissionsAsync(documentId, permissions, endusers);

                // We set individual permissions, so we should delete all enduser list permissions if any
                var listsToRemove = endusers.Select(e => e.ListId).Distinct().ToList();
                var saveChanges = false;
                foreach (var enduserListId in listsToRemove)
                {
                    if (await DeleteEnduserListPermissionFromModelAsync(documentId, enduserListId))
                        saveChanges = true;
                }

                if (saveChanges)
                    await DbContext.SaveChangesAsync();
            }

            return new ExcelUploadDto
            {
                UpdatedRecordsCount = permissions.Count,
                TotalErrorsCount = errors.TotalCount,
                Errors = errors.Errors.ToArray()
            };
        }

        private static async Task<MemoryStream> GetStream(HttpRequest request)
        {
            var stream = new MemoryStream();
            var inputStream = request.Body;
            await inputStream.CopyToAsync(stream);
            return stream;
        }

        public async Task SetEnduserListPermissionAsync(long documentId, int enduserListId, Permission permission)
        {
            if (permission == Permission.None)
            {
                await DeleteEnduserListPermissionFromModelAsync(documentId, enduserListId);
            }
            else
            {
                if (await FindOrganizationPermissionAsync(documentId, enduserListId) is EnduserListPermission entity)
                    entity.Permission = (byte) permission;
                else
                    DbContext.EnduserListPermissions.Add(
                        new EnduserListPermission
                        {
                            DocumentId = documentId,
                            OrganizationId = enduserListId,
                            Permission = (byte) permission
                        });

                await InsertSelectedEnduserListAsync(documentId, enduserListId);
            }

            await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteIndividualEnduserPermissionsAsync(long documentId, int enduserListId)
        {
            return await new QueryProvider().DeleteIndividualEnduserPermissionsAsync(documentId, enduserListId);
        }

        private async Task<bool> DeleteEnduserListPermissionFromModelAsync(long documentId, int enduserListId)
        {
            if (!(await FindOrganizationPermissionAsync(documentId, enduserListId) is EnduserListPermission entity))
                return false;

            DbContext.EnduserListPermissions.Remove(entity);
            return true;
        }

        public async Task<bool> DeleteEnduserPermissionAsync(long documentId, int enduserId)
        {
            if (!await DeleteEnduserPermissionFromModelAsync(documentId, enduserId))
                return false;

            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEnduserListPermissionAsync(long documentId, int enduserListId)
        {
            if (!await DeleteEnduserListPermissionFromModelAsync(documentId, enduserListId))
                return false;

            await DbContext.SaveChangesAsync();
            return true;
        }

        public IQueryable<OrganizationDto> GetSelectedEnduserLists(long documentId)
        {
            return
                from enduserList in DbContext.SelectedEnduserLists
                where enduserList.DocumentId == documentId
                from name in DbContext.EnduserLists
                where name.Id == enduserList.ListId
                select new OrganizationDto { Id = enduserList.ListId, Name = name.Name };
        }

        public async Task InsertSelectedEnduserListAsync(long documentId, int enduserListId)
        {
            if (await DbContext.SelectedEnduserLists.AnyAsync(l =>
                    l.DocumentId == documentId && l.ListId == enduserListId))
                // Already there
                return;

            DbContext.SelectedEnduserLists.Add(new SelectedEnduserList
            {
                DocumentId = documentId,
                ListId = enduserListId
            });
            await DbContext.SaveChangesAsync();
        }

        public async Task DeleteSelectedEnduserListAsync(long documentId, int enduserListId)
        {
            var entity = await DbContext.SelectedEnduserLists.FirstOrDefaultAsync(
                p => p.DocumentId == documentId && p.ListId == enduserListId);
            if (entity == null)
                throw new NotFoundException($"Selected enduser list {enduserListId} not found.");

            await DeleteIndividualEnduserPermissionsAsync(documentId, enduserListId);
            await DeleteEnduserListPermissionAsync(documentId, enduserListId);
            DbContext.SelectedEnduserLists.Remove(entity);

            await DbContext.SaveChangesAsync();
        }
    }
}
