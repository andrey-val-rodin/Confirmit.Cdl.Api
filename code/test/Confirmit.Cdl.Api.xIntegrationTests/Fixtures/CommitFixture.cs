using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class CommitFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;
        public long DocumentId;
        public readonly List<CommitDto> ExpectedCommits = new List<CommitDto>();

        public CommitFixture(SharedFixture sharedFixture)
        {
            _sharedFixture = sharedFixture;
        }

        protected override void AddLocalServices(IServiceCollection services)
        {
            var uri = GetServiceUri();
            services.AddConfirmitClient<ICdl>(uri);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var scope = CreateScope();
            var client = new CdlServiceClient(scope);

            await _sharedFixture.UseAdminAsync(scope);

            DocumentId = (await client.PostDocumentAsync()).Id;

            // Grant permission Manage to NormalUser
            await client.PatchUserPermissionsAsync(DocumentId,
                new[] { new UserPermissionDto { Id = _sharedFixture.NormalUser.Id, Permission = Permission.Manage } });
            // Grant permission View to CompanyAdmin
            await client.PatchUserPermissionsAsync(DocumentId,
                new[] { new UserPermissionDto { Id = _sharedFixture.CompanyAdmin.Id, Permission = Permission.View } });
            // Grant permission View to Enduser
            await client.PatchEnduserPermissionsAsync(DocumentId,
                new[] { new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View } });

            // Commit #0: Create published revision #0
            var revision0 = await client.PostRevisionAsync(DocumentId);
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                RevisionId = revision0.Id,
                RevisionNumber = revision0.Number,
                Action = Action.CreatePublished,
                CreatedBy = _sharedFixture.Admin.Id,
                Revision = new RevisionForCommitDto { CreatedBy = _sharedFixture.Admin.Id }
            });

            // Commit #1: Create snapshot revision #1
            var revision1 = await client.PostRevisionAsync(DocumentId,
                new RevisionToCreateDto { Action = ActionToCreateRevision.CreateSnapshot });
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                RevisionId = null, // RevisionId is null because revision #1 will be deleted
                RevisionNumber = revision1.Number,
                Action = Action.CreateSnapshot,
                CreatedBy = _sharedFixture.Admin.Id
            });

            // Commit #2: Create published revision #2
            var revision2 = await client.PostRevisionAsync(DocumentId);
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                RevisionId = revision2.Id,
                RevisionNumber = revision2.Number,
                Action = Action.CreatePublished,
                CreatedBy = _sharedFixture.Admin.Id,
                Revision = new RevisionForCommitDto { CreatedBy = _sharedFixture.Admin.Id }
            });

            // Commit #3: publish revision #1
            await client.PutPublishedRevisionAsync(DocumentId, new RevisionToPublishDto { Id = revision1.Id });
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                RevisionId = null, // RevisionId is null because revision #1 will be deleted
                RevisionNumber = revision1.Number,
                Action = Action.Publish,
                CreatedBy = _sharedFixture.Admin.Id
            });

            // Commit #4: delete revision #1
            await client.DeleteRevisionAsync(revision1.Id);
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                RevisionNumber = revision1.Number,
                Action = Action.Delete,
                CreatedBy = _sharedFixture.Admin.Id
            });

            // Commit #4: publish revision #2
            await client.PutPublishedRevisionAsync(DocumentId, new RevisionToPublishDto { Id = revision2.Id });
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                RevisionId = revision2.Id,
                RevisionNumber = revision2.Number,
                Action = Action.Publish,
                CreatedBy = _sharedFixture.Admin.Id,
                Revision = new RevisionForCommitDto { CreatedBy = _sharedFixture.Admin.Id }
            });

            // Commit #5: unpublish document
            await client.DeletePublishedRevisionAsync(DocumentId);
            ExpectedCommits.Add(new CommitDto
            {
                DocumentId = DocumentId,
                Action = Action.Unpublish,
                CreatedBy = _sharedFixture.Admin.Id
            });
        }
    }
}
