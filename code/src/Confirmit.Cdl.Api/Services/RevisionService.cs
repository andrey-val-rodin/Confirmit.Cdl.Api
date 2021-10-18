using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.NetCore.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Action = Confirmit.Cdl.Api.ViewModel.Action;

namespace Confirmit.Cdl.Api.Services
{
    public class RevisionService : BaseService
    {
        public RevisionService(CdlDbContext dbContext, IHttpContextAccessor httpContext,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader,
            EventService eventService, IConfirmitTokenService tokenService)
            : base(dbContext, httpContext.HttpContext.User, mapper,
                factory, accountLoader, hubPermissionReader, surveyPermissionReader)
        {
            EventService = eventService;
            TokenStore = tokenService;
        }

        private EventService EventService { get; }
        private IConfirmitTokenService TokenStore { get; }

        public Action GetAction(ActionToCreateRevision action)
        {
            // If action is not specified, use default
            if (action == 0)
                action = ActionToCreateRevision.CreatePublished;

            return action switch
            {
                ActionToCreateRevision.CreatePublished => Action.CreatePublished,
                ActionToCreateRevision.CreateSnapshot => Action.CreateSnapshot,
                _ => throw new BadRequestException("Invalid value of action")
            };
        }

        public async Task<RevisionDto> CreateRevisionAsync(
            Document document, RevisionToCreateDto revisionDto, Action action)
        {
            if (action == Action.Publish || action == Action.CreatePublished)
            {
                // When DD publishes document, he grants implicit read permissions to the hub and survey to all viewers,
                // including endusers. Therefore, we must check permissions of the current user to the hub and survey
                if (!await CheckHubPermissions(document.HubId))
                    throw new NotFoundException($"Hub {document.HubId} not found.");
                if (!await CheckSurveyPermission(document.LinkedSurveyId))
                    throw new NotFoundException($"Survey {document.LinkedSurveyId} not found.");
            }

            var revision = Mapper.Map<RevisionToCreateDto, Revision>(revisionDto);

            revision.DocumentId = document.Id;
            if (string.IsNullOrEmpty(revision.Name))
                revision.Name = document.Name;
            if (revision.SourceCode == null)
                revision.SourceCode = document.SourceCode;
            if (revision.PublicMetadata == null)
                revision.PublicMetadata = document.PublicMetadata;
            if (revision.PrivateMetadata == null)
                revision.PrivateMetadata = document.PrivateMetadata;

            var timestamp = GetTimestamp();
            SetCreatedFields(revision, timestamp);
            revision.Number = await DbContext.Revisions.CountAsync(r => r.DocumentId == document.Id) + 1;

            DbContext.Revisions.Add(revision);
            await DbContext.SaveChangesAsync();

            EventService.Issue(document, revision, EventKind.Revision, EventAction.Created, GetUserId());

            if (action == Action.CreatePublished)
            {
                document.PublishedRevisionId = revision.Id;
                await DbContext.SaveChangesAsync();

                EventService.Issue(document, EventAction.Published, GetUserId());
                EventService.Issue(document, revision, EventKind.Revision, EventAction.Published, GetUserId());

                var privateMetadata = ParsePrivateMetadata(revision.PrivateMetadata);
                if (privateMetadata?.Tags != null && privateMetadata.Tags.Contains("vault"))
                    EventService.Issue(document, TokenStore.GetToken());
            }

            return await RevisionToDtoAsync(revision);
        }

        private void SetCreatedFields(Revision revision, DateTime timestamp)
        {
            revision.CreatedBy = GetUserId();
            revision.CreatedByName = GetUserName();
            revision.Created = timestamp;
        }

        public async Task<bool> DeleteRevisionAsync(Revision revision)
        {
            var document = await GetBareDocumentByIdAsync(revision.DocumentId);
            if (document == null)
                return false;

            await new QueryProvider().RemoveReferencesToRevisionAsync(revision.Id);
            DbContext.Revisions.Remove(revision);

            var dismissPublishing = document.PublishedRevisionId == revision.Id;
            if (dismissPublishing)
            {
                // document is not the result of LINQ query with POCO object, so attach it to DbContext
                DbContext.Documents.Attach(document);
                document.PublishedRevisionId = null;
                // Tell EF what properties have been changed. Only these properties will be updated in SQL query
                DbContext.Entry(document).Property(d => d.PublishedRevisionId).IsModified = true;
            }

            await DbContext.SaveChangesAsync();

            if (dismissPublishing)
            {
                EventService.Issue(document, revision, EventKind.Revision, EventAction.Dismissed, GetUserId());
                EventService.Issue(document, EventAction.Dismissed, GetUserId());

            }

            EventService.Issue(document, revision, EventKind.Revision, EventAction.Deleted, GetUserId());

            return true;
        }

        public async Task<string> GetPublicMetadataAsync(long revisionId)
        {
            var list = await DbContext.Revisions.Where(d => d.Id == revisionId).Select(d => d.PublicMetadata)
                .ToListAsync();
            return list.Count == 1 ? list[0] : null;
        }

        public async Task<IQueryable<RevisionShortDto>> GetPublishedRevisionsAsync()
        {
            return await GetQueryForPublishedRevisionsAsync();
        }

        public async Task<IQueryable<AccessedRevisionDto>> GetAccessedRevisionsAsync()
        {
            return await GetQueryForAccessedRevisionsAsync();
        }

        public async Task<IQueryable<RevisionShortDto>> GetDocumentRevisionsAsync(long documentId)
        {
            return await GetQueryForDocumentRevisionsAsync(documentId);
        }

        private async Task<IQueryable<RevisionShortDto>> GetQueryForPublishedRevisionsAsync()
        {
            return
                from document in await GetInitialQueryForAvailableDocumentsAsync()
                where document.Resource.PublishedRevisionId != null
                from revision in DbContext.Revisions
                where revision.DocumentId == document.Resource.Id &&
                      revision.Id == document.Resource.PublishedRevisionId
                select new RevisionShortDto
                {
                    Id = revision.Id,
                    DocumentId = document.Resource.Id,
                    Type = (DocumentType) document.Resource.Type,
                    Number = revision.Number,
                    Name = revision.Name,
                    IsPublished = true,
                    CompanyId = document.Resource.CompanyId,
                    CompanyName = document.Resource.CompanyName,
                    Created = revision.Created,
                    CreatedBy = revision.CreatedBy,
                    CreatedByName = revision.CreatedByName,
                    PublicMetadata = revision.PublicMetadata,
                    PrivateMetadata = revision.PrivateMetadata,
                    Permission = document.Permission
                };
        }

        private async Task<IQueryable<AccessedRevisionDto>> GetQueryForAccessedRevisionsAsync()
        {
            var customer = await Customer;
            var userId = customer.Id;
            var isUser = customer.UserType == UserType.User;
            var baseQuery = customer.DocumentAccessor.GetQuery();

            return
                from document in baseQuery
                join accessedDocument in DbContext.AccessedDocuments
                        .Where(d => d.UserId == userId && d.IsUser == isUser)
                    on document.Resource.Id equals accessedDocument.Id into accessedDocuments
                from accessedDocument in accessedDocuments.DefaultIfEmpty()
                from revision in DbContext.Revisions
                where revision.DocumentId == document.Resource.Id &&
                      revision.Id == document.Resource.PublishedRevisionId
                select new AccessedRevisionDto
                {
                    Id = revision.Id,
                    DocumentId = document.Resource.Id,
                    Type = (DocumentType) document.Resource.Type,
                    Number = revision.Number,
                    Name = revision.Name,
                    IsPublished = document.Resource.PublishedRevisionId == revision.Id,
                    CompanyId = document.Resource.CompanyId,
                    CompanyName = document.Resource.CompanyName,
                    Accessed = accessedDocument.Accessed,
                    Created = revision.Created,
                    CreatedBy = revision.CreatedBy,
                    CreatedByName = revision.CreatedByName,
                    PublicMetadata = revision.PublicMetadata,
                    PrivateMetadata = revision.PrivateMetadata,
                    Permission = document.Permission
                };
        }

        private async Task<IQueryable<RevisionShortDto>> GetQueryForDocumentRevisionsAsync(long documentId)
        {
            return
                from document in await GetInitialQueryForAvailableDocumentsAsync()
                where document.Resource.Id == documentId
                from revision in DbContext.Revisions
                where revision.DocumentId == document.Resource.Id
                select new RevisionShortDto
                {
                    Id = revision.Id,
                    DocumentId = document.Resource.Id,
                    Type = (DocumentType) document.Resource.Type,
                    Number = revision.Number,
                    Name = revision.Name,
                    IsPublished = document.Resource.PublishedRevisionId == revision.Id,
                    CompanyId = document.Resource.CompanyId,
                    CompanyName = document.Resource.CompanyName,
                    Created = revision.Created,
                    CreatedBy = revision.CreatedBy,
                    CreatedByName = revision.CreatedByName,
                    PublicMetadata = revision.PublicMetadata,
                    PrivateMetadata = revision.PrivateMetadata,
                    Permission = document.Permission
                };
        }

        protected PrivateMetadata ParsePrivateMetadata(string privateMetadata)
        {
            if (string.IsNullOrEmpty(privateMetadata))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<PrivateMetadata>(privateMetadata);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        #region Internal classes

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        protected class PrivateMetadata
        {
            public string[] Tags { get; set; }
        }

        #endregion
    }
}
