using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class LinkFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;
        public readonly string Name = Guid.NewGuid().ToString();
        public long DocumentId;
        public long RevisionId;
        public long BareDocumentId;
        public AliasDto Alias;

        public LinkFixture(SharedFixture sharedFixture)
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

            DocumentId = (await client.PostDocumentAsync(
                new DocumentToCreateDto { Name = Name + "first" })).Id;

            // Grant permission View to NormalUser
            await client.PatchUserPermissionsAsync(DocumentId,
                new[]
                {
                    new UserPermissionDto { Id = _sharedFixture.NormalUser.Id, Permission = Permission.View }
                });

            // Create published revision (commit will be created also)
            RevisionId = (await client.PostRevisionAsync(DocumentId)).Id;

            // Create snapshot revision
            var id = (await client.PostRevisionAsync(DocumentId, new RevisionToCreateDto
            {
                Action = ActionToCreateRevision.CreateSnapshot
            })).Id;

            // Now delete snapshot revision (corresponded commit will not contain revision Id)
            await client.DeleteRevisionAsync(id);

            // Create alias
            Alias = await client.PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "LinksTests",
                Alias = "TestDocument",
                DocumentId = DocumentId
            });

            // Create document without commits, revisions and aliases
            BareDocumentId = (await client.PostDocumentAsync(
                new DocumentToCreateDto { Name = Name + "second" })).Id;

            // Third document
            await client.PostDocumentAsync(new DocumentToCreateDto { Name = Name + "third" });

            // Create and then delete two documents
            id = (await client.PostDocumentAsync(new DocumentToCreateDto { Name = Name + "4" })).Id;
            await client.DeleteDocumentAsync(id);
            id = (await client.PostDocumentAsync(new DocumentToCreateDto { Name = Name + "5" })).Id;
            await client.DeleteDocumentAsync(id);
        }
    }
}
