using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class ArchivedDocumentFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;

        public DocumentDto Document;

        public ArchivedDocumentFixture(SharedFixture sharedFixture)
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

            await _sharedFixture.UseNormalUserAsync(scope);

            Document = await client.PostDocumentAsync(new DocumentToCreateDto
            {
                Name = "ArchivedDocumentTests",
                PublicMetadata = "{\"portalId\": 456}"
            });

            // Document is in list of accessed documents
            Assert.Contains(
                (await client.GetAccessedDocumentsAsync(
                    filter: $"Name eq '{CdlServiceClient.Prefix + "ArchivedDocumentTests"}'"))
                .Items, d => d.Id == Document.Id);

            // Create alias
            await client.PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "ArchivedDocumentTests",
                Alias = "my_doc",
                DocumentId = Document.Id
            });

            // Alias exists
            Assert.Contains(
                (await client.GetAliasesAsync(filter: "Namespace eq 'ArchivedDocumentTests'")).Items,
                a => a.Namespace == "archiveddocumenttests" && a.Alias == "my_doc");

            // It is possible to get document by alias
            await client.GetDocumentByAliasAsync("ArchivedDocumentTests", "my_doc");

            // Create few commits
            await client.PostRevisionAsync(Document.Id);
            await client.PostRevisionAsync(Document.Id, new RevisionToCreateDto
            {
                Action = ActionToCreateRevision.CreateSnapshot
            });

            // There are commits
            Assert.NotEmpty((await client.GetCommitsAsync(Document.Id)).Items);
            // There is published revision
            Assert.Contains((await client.GetPublishedRevisionsAsync()).Items, r => r.DocumentId == Document.Id);
            // There is published revision
            await client.GetPublishedRevisionAsync(Document.Id);

            // Take fresh copy of document
            Document = await client.GetDocumentAsync(Document.Id);
        }
    }
}
