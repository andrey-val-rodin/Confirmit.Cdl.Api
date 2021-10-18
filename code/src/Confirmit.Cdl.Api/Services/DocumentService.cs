using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Action = Confirmit.Cdl.Api.ViewModel.Action;

namespace Confirmit.Cdl.Api.Services
{
    public class DocumentService : BaseService
    {
        private readonly EventService _eventService;
        private readonly IOptions<CleanupConfig> _cleanupConfig;

        public DocumentService(CdlDbContext dbContext, IHttpContextAccessor httpContext,
            IMapper mapper, Factory factory, IAccountLoader accountLoader, EventService eventService,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader,
            IOptions<CleanupConfig> cleanupConfig)
            : base(dbContext, httpContext.HttpContext.User, mapper,
                factory, accountLoader, hubPermissionReader, surveyPermissionReader)
        {
            _eventService = eventService;
            _cleanupConfig = cleanupConfig;
        }

        public async Task<DocumentDto> CreateDocumentAsync(DocumentToCreateDto documentDto)
        {
            var document = Mapper.Map<DocumentToCreateDto, Document>(documentDto);

            await ValidateDocumentAsync(document);

            if (documentDto.Type == DocumentType.NotSpecified)
                document.Type = (byte) DocumentType.ReportingDashboard;

            var timestamp = GetTimestamp();
            SetCreatedFields(document, timestamp);
            SetModifiedFields(document, timestamp);

            DbContext.Documents.Add(document);
            await DbContext.SaveChangesAsync();

            _eventService.Issue(document, EventAction.Created, GetUserId());

            return await DocumentToDtoAsync(document);
        }

        private async Task ValidateDocumentAsync(Document document)
        {
            if (string.IsNullOrEmpty(document.Name))
                throw new BadRequestException("Please provide document name.");
            if (document.SourceCode == null)
                throw new BadRequestException("Please provide source code.");

            var companyId = await ValidateCompanyAsync(document.CompanyId);

            document.CompanyId = companyId;
            document.CompanyName = await GetCompanyNameAsync(companyId);

            if (!await CheckHubPermissions(document.HubId))
                throw new NotFoundException($"Hub {document.HubId} not found.");
            if (!await CheckSurveyPermission(document.LinkedSurveyId))
                throw new NotFoundException($"Survey {document.LinkedSurveyId} not found.");
        }

        public async Task<DocumentDto> UpdateDocumentAsync(long documentId, DocumentPatchDto patch)
        {
            var document = await GetDocumentByIdAsync(documentId);
            if (document == null)
                throw new NotFoundException($"Document {documentId} not found.");

            await ValidatePatchAsync(patch);

            if (patch.Name != null)
                document.Name = patch.Name;

            var eventAction = EventAction.Updated;
            Revision revision = null;
            if (patch.Type > 0)
            {
                if (document.Type != (byte) patch.Type)
                {
                    if (document.PublishedRevisionId.HasValue)
                    {
                        revision = await GetRevisionByIdAsync(document.PublishedRevisionId.Value);
                        if (revision != null)
                        {
                            _eventService.Issue(
                                document, revision, EventKind.Revision, EventAction.Dismissed, GetUserId());
                            _eventService.Issue(document, EventAction.Dismissed, GetUserId());
                        }
                    }

                    _eventService.Issue(document, EventAction.Deleted, GetUserId());
                    eventAction = EventAction.Created;
                }

                document.Type = (byte) patch.Type;
            }

            if (patch.SourceCode != null)
                document.SourceCode = patch.SourceCode;

            if (patch.CompanyId > 0)
            {
                document.CompanyId = patch.CompanyId;
                document.CompanyName = await GetCompanyNameAsync(document.CompanyId);
            }

            if (patch.SourceCodeEditOps != null)
                document.SourceCodeEditOps = patch.SourceCodeEditOps;

            if (patch.PublicMetadata != null)
                document.PublicMetadata = patch.PublicMetadata;

            if (patch.PrivateMetadata != null)
                document.PrivateMetadata = patch.PrivateMetadata;

            if (patch.HubId != null)
                document.HubId = patch.HubId;

            if (patch.LinkedSurveyId != null)
                document.LinkedSurveyId = patch.LinkedSurveyId;

            if (patch.OriginDocumentId != null)
                document.OriginDocumentId = patch.OriginDocumentId;

            var timestamp = GetTimestamp();
            SetModifiedFields(document, timestamp);

            await DbContext.SaveChangesAsync();

            _eventService.Issue(document, eventAction, GetUserId());
            if (revision != null)
            {
                _eventService.Issue(document, EventAction.Published, GetUserId());
                _eventService.Issue(document, revision, EventKind.Revision, EventAction.Published, GetUserId());
            }

            return await DocumentToDtoAsync(document);
        }

        private async Task ValidatePatchAsync(DocumentPatchDto patch)
        {
            if (patch.Name == string.Empty)
                throw new BadRequestException("Document name cannot be empty.");

            await ValidateCompanyAsync(patch.CompanyId);

            if (patch.HubId != null)
            {
                if (!await CheckHubPermissions(patch.HubId))
                    throw new NotFoundException($"Hub {patch.HubId} not found.");
            }

            if (patch.LinkedSurveyId != null)
            {
                if (patch.LinkedSurveyId == string.Empty)
                    throw new BadRequestException("Survey cannot be empty.");

                if (!await CheckSurveyPermission(patch.LinkedSurveyId))
                    throw new NotFoundException($"Survey {patch.LinkedSurveyId} not found.");
            }
        }

        public async Task PublishDocumentAsync(Document document, RevisionToPublishDto revisionDto)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (revisionDto == null)
                throw new BadRequestException("Please specify revision.");

            // When DD publishes document, he grants implicit read permissions to the hub and survey to all viewers,
            // including endusers. Therefore, we must check permissions of the current user to the hub and survey
            if (!await CheckHubPermissions(document.HubId))
                throw new NotFoundException($"Hub {document.HubId} not found.");
            if (!await CheckSurveyPermission(document.LinkedSurveyId))
                throw new NotFoundException($"Survey {document.LinkedSurveyId} not found.");

            var revision = await GetRevisionByIdAsync(revisionDto.Id);
            if (revision == null || revision.DocumentId != document.Id)
                throw new NotFoundException($"Revision {revisionDto.Id} not found.");

            // document may not be the result of LINQ query with POCO object, so attach it to DbContext
            DbContext.Documents.Attach(document);
            document.PublishedRevisionId = revision.Id;
            SetModifiedFields(document, GetTimestamp());
            // Tell EF what properties have been changed. Only these properties will be updated in SQL query
            DbContext.Entry(document).Property(d => d.PublishedRevisionId).IsModified = true;
            DbContext.Entry(document).Property(d => d.Modified).IsModified = true;
            DbContext.Entry(document).Property(d => d.ModifiedBy).IsModified = true;
            DbContext.Entry(document).Property(d => d.ModifiedByName).IsModified = true;

            DbContext.Commits.Add(CreateCommit(document.Id, revision.Id, revision.Number, Action.Publish,
                document.Modified));

            await DbContext.SaveChangesAsync();

            _eventService.Issue(document, EventAction.Published, GetUserId());
            _eventService.Issue(document, revision, EventKind.Revision, EventAction.Published, GetUserId());
        }

        public async Task UnpublishDocumentAsync(Document document)
        {
            if (document.PublishedRevisionId == null)
                throw new BadRequestException("Document is not published");

            var revision = await GetRevisionByIdAsync(document.PublishedRevisionId.Value);

            // document may not be the result of LINQ query with POCO object, so attach it to DbContext
            DbContext.Documents.Attach(document);
            document.PublishedRevisionId = null;
            SetModifiedFields(document, GetTimestamp());
            // Tell EF what properties have been changed. Only these properties will be updated in SQL query
            DbContext.Entry(document).Property(d => d.PublishedRevisionId).IsModified = true;
            DbContext.Entry(document).Property(d => d.Modified).IsModified = true;
            DbContext.Entry(document).Property(d => d.ModifiedBy).IsModified = true;
            DbContext.Entry(document).Property(d => d.ModifiedByName).IsModified = true;

            DbContext.Commits.Add(CreateCommit(document.Id, null, null, Action.Unpublish, document.Modified));

            await DbContext.SaveChangesAsync();

            if (revision != null)
            {
                _eventService.Issue(document, revision, EventKind.Revision, EventAction.Dismissed, GetUserId());
            }

            _eventService.Issue(document, EventAction.Dismissed, GetUserId());
        }

        public async Task<bool> DeleteDocumentAsync(long documentId)
        {
            var document = await GetBareDocumentByIdAsync(documentId);
            if (document == null)
                return false;

            // document is not the result of LINQ query with POCO object, so attach it to DbContext
            DbContext.Documents.Attach(document);

            var timestamp = GetTimestamp();

            var commitOnDelete = CreateCommit(document.Id, null, null, Action.Delete, timestamp);
            DbContext.Commits.Add(commitOnDelete);

            SetDeletedFields(document, GetTimestamp());

            await DbContext.SaveChangesAsync();

            if (document.PublishedRevisionId.HasValue)
            {
                var revision = await GetRevisionByIdAsync(document.PublishedRevisionId.Value);
                if (revision != null)
                {
                    _eventService.Issue(document, revision, EventKind.Revision, EventAction.Dismissed, GetUserId());
                    _eventService.Issue(document, EventAction.Dismissed, GetUserId());
                }
            }

            _eventService.Issue(document, EventAction.Deleted, GetUserId());

            return true;
        }

        public async Task<DocumentDto> RestoreDocumentAsync(long documentId)
        {
            var document = await GetDeletedDocuments().FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
                return null;

            ResetDeletedFields(document);
            var commitOnRestore = CreateCommit(document.Id, null, null, Action.Restore, GetTimestamp());
            DbContext.Commits.Add(commitOnRestore);

            await DbContext.SaveChangesAsync();

            _eventService.Issue(document, EventAction.Created, GetUserId());

            if (document.PublishedRevisionId.HasValue)
            {
                var revision = await GetRevisionByIdAsync(document.PublishedRevisionId.Value);
                if (revision != null)
                {
                    _eventService.Issue(document, EventAction.Published, GetUserId());
                    _eventService.Issue(document, revision, EventKind.Revision, EventAction.Published, GetUserId());
                }
            }

            return await DocumentToDtoAsync(document);
        }

        public async Task<bool> PhysicallyDeleteDocumentAsync(long documentId)
        {
            var document = await GetDeletedDocuments().FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
                return false;

            try
            {
                if (await new QueryProvider().PhysicallyDeleteDocumentAsync(documentId) != 1)
                    return false;
            }
            catch (Exception e)
            {
                // Assume we caught deadlock
                throw new DeadlockException("Deadlock when physically deleting document", e);
            }

            return true;
        }

        private void SetCreatedFields(Document document, DateTime timestamp)
        {
            document.CreatedBy = GetUserId();
            document.CreatedByName = GetUserName();
            document.Created = timestamp;
        }

        private void SetModifiedFields(Document document, DateTime timestamp)
        {
            document.ModifiedBy = GetUserId();
            document.ModifiedByName = GetUserName();
            document.Modified = timestamp;
        }

        private void SetDeletedFields(Document document, DateTime timestamp)
        {
            document.DeletedBy = GetUserId();
            document.DeletedByName = GetUserName();
            document.Deleted = timestamp;
        }

        private void ResetDeletedFields(Document document)
        {
            document.DeletedBy = null;
            document.DeletedByName = null;
            document.Deleted = null;
        }

        public async Task<IQueryable<DocumentShortDto>> GetDocumentsAsync()
        {
            return await GetDocumentsQueryAsync();
        }

        public async Task<string> GetPublicMetadataAsync(long documentId)
        {
            var list = await GetAvailableDocuments().Where(d => d.Id == documentId).Select(d => d.PublicMetadata)
                .ToListAsync();
            return list.Count == 1 ? list[0] : null;
        }

        public async Task<IQueryable<AccessedDocumentDto>> GetAccessedDocumentsAsync()
        {
            return await GetAccessedDocumentsQueryAsync();
        }

        public async Task<IQueryable<DocumentShortDto>> GetDeletedDocumentsAsync()
        {
            return await GetDeletedDocumentsQueryAsync();
        }

        public async Task<DocumentDto> GetDocumentDtoByIdAsync(long documentId)
        {
            var document = await GetDocumentByIdAsync(documentId);
            return document == null ? null : await DocumentToDtoAsync(document);
        }

        public async Task<DocumentDto> GetDeletedDocumentDtoByIdAsync(long documentId)
        {
            var document = await GetDeletedDocuments().FirstOrDefaultAsync(d => d.Id == documentId);
            return document == null ? null : await DocumentToDtoAsync(document);
        }

        private async Task<IQueryable<DocumentShortDto>> GetDocumentsQueryAsync()
        {
            return
                from document in await GetInitialQueryForAvailableDocumentsAsync()
                select new DocumentShortDto
                {
                    Id = document.Resource.Id,
                    Name = document.Resource.Name,
                    Type = (DocumentType) document.Resource.Type,
                    CompanyId = document.Resource.CompanyId,
                    CompanyName = document.Resource.CompanyName,
                    Created = document.Resource.Created,
                    CreatedBy = document.Resource.CreatedBy,
                    CreatedByName = document.Resource.CreatedByName,
                    Modified = document.Resource.Modified,
                    ModifiedBy = document.Resource.ModifiedBy,
                    ModifiedByName = document.Resource.ModifiedByName,
                    PublishedRevisionId = document.Resource.PublishedRevisionId,
                    PublicMetadata = document.Resource.PublicMetadata,
                    PrivateMetadata = document.Resource.PrivateMetadata,
                    HubId = document.Resource.HubId,
                    LinkedSurveyId = document.Resource.LinkedSurveyId,
                    OriginDocumentId = document.Resource.OriginDocumentId,
                    Permission = document.Permission
                };
        }

        private async Task<IQueryable<AccessedDocumentDto>> GetAccessedDocumentsQueryAsync()
        {
            var customer = await Customer;
            var userId = customer.Id;
            var isUser = customer.UserType == UserType.User;
            var baseQuery = GetInitialQueryForAvailableDocuments(customer);

            return
                from document in baseQuery
                join accessedDocument in DbContext.AccessedDocuments
                        .Where(d => d.UserId == userId && d.IsUser == isUser)
                    on document.Resource.Id equals accessedDocument.Id into accessedDocuments
                from accessedDocument in accessedDocuments.DefaultIfEmpty()
                select new AccessedDocumentDto
                {
                    Id = document.Resource.Id,
                    Name = document.Resource.Name,
                    Type = (DocumentType) document.Resource.Type,
                    CompanyId = document.Resource.CompanyId,
                    CompanyName = document.Resource.CompanyName,
                    Accessed = accessedDocument.Accessed,
                    Created = document.Resource.Created,
                    CreatedBy = document.Resource.CreatedBy,
                    CreatedByName = document.Resource.CreatedByName,
                    Modified = document.Resource.Modified,
                    ModifiedBy = document.Resource.ModifiedBy,
                    ModifiedByName = document.Resource.ModifiedByName,
                    PublishedRevisionId = document.Resource.PublishedRevisionId,
                    PublicMetadata = document.Resource.PublicMetadata,
                    PrivateMetadata = document.Resource.PrivateMetadata,
                    HubId = document.Resource.HubId,
                    LinkedSurveyId = document.Resource.LinkedSurveyId,
                    OriginDocumentId = document.Resource.OriginDocumentId,
                    Permission = document.Permission
                };
        }

        private async Task<IQueryable<DocumentShortDto>> GetDeletedDocumentsQueryAsync()
        {
            return
                from document in await GetInitialQueryDeletedDocumentsAsync()
                select new DocumentShortDto
                {
                    Id = document.Resource.Id,
                    Name = document.Resource.Name,
                    Type = (DocumentType) document.Resource.Type,
                    CompanyId = document.Resource.CompanyId,
                    CompanyName = document.Resource.CompanyName,
                    Created = document.Resource.Created,
                    CreatedBy = document.Resource.CreatedBy,
                    CreatedByName = document.Resource.CreatedByName,
                    Modified = document.Resource.Modified,
                    ModifiedBy = document.Resource.ModifiedBy,
                    ModifiedByName = document.Resource.ModifiedByName,
                    Deleted = document.Resource.Deleted,
                    DeletedBy = document.Resource.DeletedBy,
                    DeletedByName = document.Resource.DeletedByName,
                    PublishedRevisionId = document.Resource.PublishedRevisionId,
                    PublicMetadata = document.Resource.PublicMetadata,
                    PrivateMetadata = document.Resource.PrivateMetadata,
                    HubId = document.Resource.HubId,
                    LinkedSurveyId = document.Resource.LinkedSurveyId,
                    OriginDocumentId = document.Resource.OriginDocumentId,
                    Permission = document.Permission
                };
        }

        public object GetArchiveLink(IUrlHelper urlHelper, long documentId)
        {
            return new
            {
                Links = new { Archive = urlHelper.RelativeLink("GetArchivedDocumentById", new { Id = documentId }) }
            };
        }

        public ExpirationPeriod GetExpirationPeriod()
        {
            return new ExpirationPeriod { ExpirationPeriodInDays = _cleanupConfig.Value.ExpirationPeriodInDays };
        }
    }
}
