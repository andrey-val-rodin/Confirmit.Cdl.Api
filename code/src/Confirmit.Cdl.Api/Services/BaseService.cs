using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.NetCore.Identity.Sdk.Claims;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Action = Confirmit.Cdl.Api.ViewModel.Action;
using Company = Confirmit.Cdl.Api.Accounts.Company;

namespace Confirmit.Cdl.Api.Services
{
    public abstract class BaseService
    {
        protected readonly CdlDbContext DbContext;
        private readonly ClaimsPrincipal _principal;
        private readonly Factory _factory;
        protected readonly IMapper Mapper;
        protected readonly IAccountLoader AccountLoader;
        public readonly DbRefresher DbRefresher;
        private readonly HubPermissionReader _hubPermissionReader;
        private readonly SurveyPermissionReader _surveyPermissionReader;
        private Task<ICustomer> _customer;

        protected BaseService(CdlDbContext dbContext, ClaimsPrincipal principal,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader)
        {
            DbContext = dbContext;
            DbRefresher = new DbRefresher(accountLoader, principal);
            _principal = principal;
            _factory = factory;
            Mapper = mapper;
            AccountLoader = accountLoader;
            _hubPermissionReader = hubPermissionReader;
            _surveyPermissionReader = surveyPermissionReader;
        }

        public Task<ICustomer> Customer => _customer ??= _factory.CreateCustomerAsync();

        public async Task<bool> IsInRoleAsync(Role role)
        {
            return (await Customer).IsInRole(role);
        }

        public async Task<bool> HasPermissionAsync(
            long documentId, Permission permission, ResourceStatus status)
        {
            return await (await Customer).DocumentAccessor.HasPermissionAsync(documentId, permission, status);
        }

        public async Task<Permission> GetPermissionAsync(
            long documentId, ResourceStatus status)
        {
            return await (await Customer).DocumentAccessor.GetPermissionAsync(documentId, status);
        }

        public async Task<DocumentAlias> GetAliasByIdAsync(long aliasId)
        {
            return await DbContext.Aliases.FirstOrDefaultAsync(a => a.Id == aliasId);
        }

        public async Task<Document> GetDocumentByIdAsync(long documentId)
        {
            return await GetAvailableDocuments().FirstOrDefaultAsync(d => d.Id == documentId);
        }

        protected IQueryable<Document> GetAvailableDocuments()
        {
            return DbContext.Documents.Where(d => d.Deleted == null);
        }

        protected IQueryable<Document> GetDeletedDocuments()
        {
            return DbContext.Documents.Where(d => d.Deleted != null);
        }

        protected async Task<IQueryable<PermittedResource<Document>>> GetInitialQueryForAvailableDocumentsAsync()
        {
            return GetInitialQueryForAvailableDocuments(await Customer);
        }

        protected static IQueryable<PermittedResource<Document>> GetInitialQueryForAvailableDocuments(
            ICustomer customer)
        {
            return customer.DocumentAccessor.GetQuery().Where(d => d.Resource.Deleted == null);
        }

        protected async Task<IQueryable<PermittedResource<Document>>> GetInitialQueryDeletedDocumentsAsync()
        {
            return GetInitialQueryForDeletedDocuments(await Customer);
        }

        private static IQueryable<PermittedResource<Document>> GetInitialQueryForDeletedDocuments(ICustomer customer)
        {
            return customer.DocumentAccessor.GetQuery().Where(d => d.Resource.Deleted != null);
        }

        public async Task<Document> GetBareDocumentByIdAsync(long documentId)
        {
            // Retrieve document without SourceCode, SourceCodeEditOps, PublicMetadata and PrivateMetadata
            // Due to EF constraints, we cannot construct POCO object in query, so use DocumentDto instead of Document
            var list = await GetAvailableDocuments().Where(d => d.Id == documentId).Select(d => new DocumentDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = (DocumentType) d.Type,
                Created = d.Created,
                CreatedBy = d.CreatedBy,
                CreatedByName = d.CreatedByName,
                Modified = d.Modified,
                ModifiedBy = d.ModifiedBy,
                ModifiedByName = d.ModifiedByName,
                CompanyId = d.CompanyId,
                CompanyName = d.CompanyName,
                HubId = d.HubId,
                LinkedSurveyId = d.LinkedSurveyId,
                PublishedRevisionId = d.PublishedRevisionId,
                OriginDocumentId = d.OriginDocumentId
            }).ToListAsync();

            if (list.Count != 1)
                return null;

            // Convert DocumentDto to Document
            return Mapper.Map<DocumentDto, Document>(list[0]);
        }

        public async Task<Document> GetBareDocumentByAliasAsync(string @namespace, string alias)
        {
            var documentAlias = await FindAliasAsync(@namespace, alias);
            if (documentAlias == null)
                return null;
            return await GetBareDocumentByIdAsync(documentAlias.DocumentId);
        }

        public async Task<Document> GetDocumentByAliasAsync(string @namespace, string alias)
        {
            var documentAlias = await FindAliasAsync(@namespace, alias);
            if (documentAlias == null)
                return null;
            return await GetDocumentByIdAsync(documentAlias.DocumentId);
        }

        protected async Task<DocumentAlias> FindAliasAsync(string @namespace, string alias)
        {
            return await DbContext.Aliases.SingleOrDefaultAsync(a => a.Namespace == @namespace && a.Alias == alias);
        }

        public async Task<DocumentDto> DocumentToDtoAsync(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var result = Mapper.Map<Document, DocumentDto>(document);
            result.CompanyName = await GetCompanyNameAsync(document.CompanyId);

            return result;
        }

        public async Task<bool> HasAliasesAsync(long documentId)
        {
            return await DbContext.Aliases.AnyAsync(a => a.DocumentId == documentId);
        }

        public async Task<bool> HasCommitsAsync(long documentId)
        {
            return await DbContext.Commits.AnyAsync(c => c.DocumentId == documentId);
        }

        public async Task<bool> HasRevisionsAsync(long documentId)
        {
            return await DbContext.Revisions.AnyAsync(r => r.DocumentId == documentId);
        }

        public async Task<RevisionDto> RevisionToDtoAsync(Revision revision)
        {
            var document = await GetBareDocumentByIdAsync(revision.DocumentId);
            if (document == null)
                return null;

            return await RevisionToDtoAsync(document, revision);
        }

        public async Task<RevisionDto> RevisionToDtoAsync(Document document, Revision revision)
        {
            var result = Mapper.Map<Revision, RevisionDto>(revision);
            result.Type = (DocumentType) document.Type;
            result.IsPublished = document.PublishedRevisionId == revision.Id;
            result.CompanyId = document.CompanyId;
            result.CompanyName = await GetCompanyNameAsync(document.CompanyId);

            return result;
        }

        public async Task<Revision> GetRevisionByIdAsync(long revisionId)
        {
            return await DbContext.Revisions.FirstOrDefaultAsync(db => db.Id == revisionId);
        }

        protected int GetUserId()
        {
            return _principal.UserId();
        }

        private int GetCompanyId()
        {
            return _principal.CompanyId();
        }

        protected async Task<string> GetCompanyNameAsync(int companyId)
        {
            var query = from company in DbContext.Companies where company.Id == companyId select company.Name;
            return await query.FirstOrDefaultAsync();
        }

        protected async Task<int> ValidateCompanyAsync(int companyId)
        {
            // 0 is treated as "no value"; take company from current principal
            if (companyId == 0)
                companyId = GetCompanyId();

            Company company;
            if (companyId == GetCompanyId())
            {
                company = new Company
                {
                    CompanyId = companyId,
                    Name = _principal.CompanyName()
                };
            }
            else
            {
                company = await AccountLoader.GetCompanyAsync(companyId);
                if (company == null)
                    throw new NotFoundException($"Company {companyId} not found.");
            }

            await DbRefresher.RefreshCompanyAsync(company);
            return companyId;
        }

        protected string GetUserName()
        {
            return _principal.FirstName() + " " + _principal.LastName();
        }

        public DateTime GetTimestamp()
        {
            // Use SqlDateTime to round-off time, as SQL Server does
            return new SqlDateTime(DateTime.UtcNow).Value;
        }

        public static Page<T> ApplyODataQueryOptions<T>(
            IQueryable<T> query, ODataQueryOptions<T> options)
        {
            try
            {
                // Total count query
                var countQuery = options.Filter == null
                    ? query
                    : (IQueryable<T>) options.Filter.ApplyTo(query, new ODataQuerySettings());
                int totalCount = countQuery.Count();

                // Items query
                query = (IQueryable<T>) options.ApplyTo(query);
                var entities = query.ToList();

                int skip = options.Skip?.Value ?? 0;
                int top = options.Top?.Value ?? entities.Count;

                // Database can be changed between two queries, so we have to validate total count
                totalCount = ValidateTotalCount(skip, top, entities.Count, totalCount);

                return new Page<T>(skip, top) { Entities = entities, TotalCount = totalCount };
            }
            catch (ODataException e)
            {
                throw new BadRequestException(e.Message);
            }
        }

        public static async Task<Page<T>> ApplyODataQueryOptionsAsync<T>(
            IQueryable<T> query, ODataQueryOptions<T> options)
        {
            try
            {
                // Total count query
                var countQuery = options.Filter == null
                    ? query
                    : (IQueryable<T>) options.Filter.ApplyTo(query, new ODataQuerySettings());
                int totalCount = await countQuery.CountAsync();

                // Items query
                query = (IQueryable<T>) options.ApplyTo(query);
                var entities = await query.ToListAsync();

                int skip = options.Skip?.Value ?? 0;
                int top = options.Top?.Value ?? entities.Count;

                // Database can be changed between two queries, so we have to validate total count
                totalCount = ValidateTotalCount(skip, top, entities.Count, totalCount);

                return new Page<T>(skip, top) { Entities = entities, TotalCount = totalCount };
            }
            catch (ODataException e)
            {
                throw new BadRequestException(e.Message);
            }
        }

        private static int ValidateTotalCount(int skip, int top, int itemCount, int totalCount)
        {
            // Preconditions: skip and top cannot be negative. OData throws exception in this case.

            if (top == 0)
                return totalCount; // Zero page requested, return total count as is

            if (itemCount == 0)
            {
                // If page is empty, then total count cannot be more than skip
                if (totalCount > skip)
                    totalCount = skip;
            }
            else
            {
                bool pageComplete = itemCount == top;
                if (pageComplete)
                {
                    // If page is complete, then total count cannot be less than skip + top
                    if (totalCount < skip + top)
                        totalCount = skip + top;
                }
                else
                {
                    // If page is not complete, then total count must be equal to skip + itemCount
                    totalCount = skip + itemCount;
                }
            }

            return totalCount;
        }

        public async Task CreateOrUpdateAccessedDocumentTimestampAsync(long documentId, DateTime timestamp)
        {
            var customer = await Customer;
            var userId = customer.Id;
            var isUser = customer.UserType == UserType.User;
            var entity = await DbContext.AccessedDocuments.FirstOrDefaultAsync(d =>
                d.Id == documentId && d.UserId == userId && d.IsUser == isUser);
            if (entity == null)
                DbContext.AccessedDocuments.Add(new AccessedDocument
                {
                    Id = documentId,
                    UserId = userId,
                    IsUser = isUser,
                    Accessed = timestamp
                });
            else
                entity.Accessed = timestamp;

            await DbContext.SaveChangesAsync();
        }

        protected Commit CreateCommit(
            long documentId, long? revisionId, int? revisionNumber, Action action, DateTime timestamp)
        {
            return new Commit
            {
                DocumentId = documentId,
                RevisionId = revisionId,
                RevisionNumber = revisionNumber,
                Action = (byte) action,
                Created = timestamp,
                CreatedBy = GetUserId(),
                CreatedByName = GetUserName()
            };
        }

        public async Task CreateAndAddCommitAsync(
            long documentId, long? revisionId, int? revisionNumber, Action action, DateTime timestamp)
        {
            var commit = CreateCommit(documentId, revisionId, revisionNumber, action, timestamp);
            DbContext.Commits.Add(commit);
            await DbContext.SaveChangesAsync();
        }

        protected async Task<bool> CheckHubPermissions(long? hubId)
        {
            if (hubId == null)
                return true;

            var permission = await _hubPermissionReader.GetPermissionAsync(hubId.Value);
            return permission != Permission.None;
        }

        protected async Task<bool> CheckSurveyPermission(string surveyId)
        {
            if (string.IsNullOrEmpty(surveyId))
                return true;

            var permission = await _surveyPermissionReader.GetPermissionAsync(surveyId);
            return permission != Permission.None;
        }

        public static string GetPageLink<T>(
            IUrlHelper urlHelper,
            string routeName,
            ODataQueryOptions<T> options,
            int? skip)
        {
            if (skip == null)
                return null;

            var routeValues = new Dictionary<string, object> { { "$skip", skip.Value } };
            if (options.Top != null)
                routeValues.Add("$top", options.Top.RawValue);
            if (options.OrderBy != null)
                routeValues.Add("$orderby", options.OrderBy.RawValue);
            if (options.Filter != null)
                routeValues.Add("$filter", options.Filter.RawValue);

            return urlHelper.RelativeLink(routeName, routeValues);
        }
    }
}
