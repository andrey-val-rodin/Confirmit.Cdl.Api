using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Confirmit.Cdl.Api.Services
{
    public class CommitService : BaseService
    {
        public CommitService(CdlDbContext dbContext, IHttpContextAccessor httpContext,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader)
            : base(dbContext, httpContext.HttpContext.User, mapper,
                factory, accountLoader, hubPermissionReader, surveyPermissionReader)
        {
        }

        public IQueryable<CommitDto> GetQuery(long documentId)
        {
            return
                from commit in DbContext.Commits
                where commit.DocumentId == documentId
                join revision in DbContext.Revisions on commit.RevisionId equals revision.Id into revisions
                from revision in revisions.DefaultIfEmpty()
                select new CommitDto
                {
                    Id = commit.Id,
                    DocumentId = commit.DocumentId,
                    RevisionId = commit.RevisionId,
                    RevisionNumber = commit.RevisionNumber,
                    Action = (Action) commit.Action,
                    Created = commit.Created,
                    CreatedBy = commit.CreatedBy,
                    CreatedByName = commit.CreatedByName,
                    Revision = revision == null
                        ? null
                        : new RevisionForCommitDto
                        {
                            Created = revision.Created,
                            CreatedBy = revision.CreatedBy,
                            CreatedByName = revision.CreatedByName,
                            PublicMetadata = revision.PublicMetadata,
                            PrivateMetadata = revision.PrivateMetadata
                        }
                };
        }
    }
}
