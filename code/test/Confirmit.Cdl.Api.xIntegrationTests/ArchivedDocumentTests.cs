using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Action = Confirmit.Cdl.Api.ViewModel.Action;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class ArchivedDocumentTests : TestBase, IClassFixture<ArchivedDocumentFixture>, IAsyncLifetime
    {
        private readonly ArchivedDocumentFixture _fixture;

        public ArchivedDocumentTests(SharedFixture sharedFixture, ArchivedDocumentFixture fixture,
            ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DeleteDocument_GetDocument_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await DeleteDocumentAsync(id); // successfully deleted

            await GetDocumentAsync(id, HttpStatusCode.NotFound); // document no longer exists
        }

        [Fact]
        public async Task DeleteDocument_PatchDocument_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await DeleteDocumentAsync(id); // successfully deleted

            await PatchDocumentAsync(id, new DocumentPatchDto(), HttpStatusCode.NotFound); // document no longer exists
        }

        [Fact]
        public async Task DeleteDocument_DeleteDocument_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await DeleteDocumentAsync(id); // successfully deleted

            await DeleteDocumentAsync(id, HttpStatusCode.NotFound); // document no longer exists
        }

        [Fact]
        public async Task DeleteDocument_GetAccessedDocuments_NoDocument()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await DeleteDocumentAsync(id); // successfully deleted

            Assert.DoesNotContain(
                (await GetAccessedDocumentsAsync(filter: $"Name eq '{Prefix + "ArchivedDocumentTests"}'")).Items,
                d => d.Id == id);
        }

        [Fact]
        public async Task DeleteDocument_GetArchivedDocuments_ContainsValidDocument()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            var archivedDocument =
                (await GetArchivedDocumentsAsync()).Items.SingleOrDefault(d => d.Id == _fixture.Document.Id);

            AssertValidArchivedDocument(archivedDocument);
        }

        [Fact]
        public async Task DeleteDocument_GetArchivedDocument_ValidDocument()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            var archivedDocument = await GetArchivedDocumentAsync(_fixture.Document.Id);

            AssertValidArchivedDocument(archivedDocument);
        }

        [Fact]
        public async Task DeleteDocument_GetRevisions_NotFound()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            await GetRevisionsAsync(_fixture.Document.Id, null, expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteDocument_GetPublishedRevisions_NoRevision()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            Assert.DoesNotContain((await GetPublishedRevisionsAsync()).Items,
                r => r.DocumentId == _fixture.Document.Id);
        }

        [Fact]
        public async Task DeleteDocument_GetPublishedRevision_NotFound()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            await GetPublishedRevisionAsync(_fixture.Document.Id, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteDocument_GetAliases_NoAlias()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            // Alias does not exists
            Assert.DoesNotContain((await GetAliasesAsync(filter: "Namespace eq 'ArchivedDocumentTests'")).Items,
                a => a.Namespace == "archiveddocumenttests" && a.Alias == "my_doc");
        }

        [Fact]
        public async Task DeleteDocument_GetDocumentByAlias_NotFound()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            await GetDocumentByAliasAsync("ArchivedDocumentTests", "my_doc",
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteDocument_PostTheSameAlias_NoAlias()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            var id = (await PostDocumentAsync()).Id;
            await PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "ArchivedDocumentTests",
                Alias = "my_doc",
                DocumentId = id
            }, HttpStatusCode.BadRequest, "Unable to set new alias. Try different namespace or alias name");
        }

        [Fact]
        public async Task DeleteDocument_GetCommits_NotFound()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            await GetCommitsAsync(_fixture.Document.Id, expectedStatusCode: HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RestoreDocument_DocumentIsTheSame()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            var restoredDocument = await RestoreArchivedDocumentAsync(_fixture.Document.Id);

            Assert.Equal(
                Newtonsoft.Json.JsonConvert.SerializeObject(_fixture.Document),
                Newtonsoft.Json.JsonConvert.SerializeObject(restoredDocument));
        }

        [Fact]
        public async Task RestoreDocument_GetCommits_ThereIsCommitDelete()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            await RestoreArchivedDocumentAsync(_fixture.Document.Id);
            var commits = await GetCommitsAsync(_fixture.Document.Id);

            Assert.Contains(commits.Items, c => c.Action == Action.Delete && c.Revision == null);
        }

        [Fact]
        public async Task RestoreDocument_GetCommits_LastCommitIsRestore()
        {
            await UseNormalUserAsync();
            await DeleteDocumentAsync(_fixture.Document.Id);

            await RestoreArchivedDocumentAsync(_fixture.Document.Id);
            var lastCommit = (await GetCommitsAsync(_fixture.Document.Id)).Items.LastOrDefault();

            Assert.NotNull(lastCommit);
            Assert.Equal(Action.Restore, lastCommit.Action);
        }

        [Fact]
        public async Task DeleteArchivedDocumentAsync_NoMoreDocument()
        {
            var name = GenerateRandomName();

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;
            await DeleteDocumentAsync(id);
            await DeleteArchivedDocumentAsync(id);

            var existentDocuments = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var archivedDocuments = (await GetArchivedDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;

            Assert.DoesNotContain(existentDocuments, d => d.Id == id);
            Assert.DoesNotContain(archivedDocuments, d => d.Id == id);
        }

        [Fact]
        public async Task GetExpirationPeriod_ValidValue()
        {
            await UseNormalUserAsync();

            var result = (await GetExpirationPeriodAsync()).ExpirationPeriodInDays;

            Assert.True(result >= 1 && result <= 100, "Reasonable value");
        }

        #region Helpers

        private async Task<bool> IsDocumentDeletedAsync(long id)
        {
            var response = await Service.GetArchivedDocumentAsync(id);
            return response.StatusCode == HttpStatusCode.OK;
        }

        private void AssertValidArchivedDocument(DocumentShortDto doc)
        {
            Assert.NotNull(doc.Deleted);
            Assert.True(doc.Modified < doc.Deleted, "Modification time must be less than deletion time");
            Assert.True(doc.Deleted < new SqlDateTime(DateTime.UtcNow).Value + TimeSpan.FromMinutes(1),
                "Deletion time must be less than current time + 1 minute (time on server can differ from local time");
            Assert.True(NormalUser.Id == doc.DeletedBy, "Deleted by normal user");
            Assert.True(NormalUser.FullName == doc.DeletedByName, "normal user name");

            Assert.Equal(_fixture.Document.Id, doc.Id);
            Assert.Equal(_fixture.Document.Name, doc.Name);
            Assert.Equal(_fixture.Document.CompanyId, doc.CompanyId);
            Assert.Equal(_fixture.Document.CompanyName, doc.CompanyName);
            Assert.Equal(_fixture.Document.Created, doc.Created);
            Assert.Equal(_fixture.Document.CreatedBy, doc.CreatedBy);
            Assert.Equal(_fixture.Document.CreatedByName, doc.CreatedByName);
            Assert.Equal(_fixture.Document.Modified, doc.Modified);
            Assert.Equal(_fixture.Document.ModifiedBy, doc.ModifiedBy);
            Assert.Equal(_fixture.Document.ModifiedByName, doc.ModifiedByName);
            Assert.Equal(_fixture.Document.PublishedRevisionId, doc.PublishedRevisionId);
            Assert.Equal(_fixture.Document.PublicMetadata, doc.PublicMetadata);
            Assert.Equal(_fixture.Document.PrivateMetadata, doc.PrivateMetadata);
        }

        private void AssertValidArchivedDocument(DocumentDto doc)
        {
            Assert.NotNull(doc.Deleted);
            Assert.True(doc.Modified < doc.Deleted, "Modification time must be less than deletion time");
            Assert.True(doc.Deleted < new SqlDateTime(DateTime.UtcNow).Value + TimeSpan.FromMinutes(1),
                "Deletion time must be less than current time + 1 minute (time on server can differ from local time");
            Assert.True(NormalUser.Id == doc.DeletedBy, "Deleted by normal user");
            Assert.True(NormalUser.FullName == doc.DeletedByName, "normal user name");

            Assert.Equal(_fixture.Document.Id, doc.Id);
            Assert.Equal(_fixture.Document.Name, doc.Name);
            Assert.Equal(_fixture.Document.CompanyId, doc.CompanyId);
            Assert.Equal(_fixture.Document.CompanyName, doc.CompanyName);
            Assert.Equal(_fixture.Document.Created, doc.Created);
            Assert.Equal(_fixture.Document.CreatedBy, doc.CreatedBy);
            Assert.Equal(_fixture.Document.CreatedByName, doc.CreatedByName);
            Assert.Equal(_fixture.Document.Modified, doc.Modified);
            Assert.Equal(_fixture.Document.ModifiedBy, doc.ModifiedBy);
            Assert.Equal(_fixture.Document.ModifiedByName, doc.ModifiedByName);
            Assert.Equal(_fixture.Document.PublishedRevisionId, doc.PublishedRevisionId);
            Assert.Equal(_fixture.Document.PublicMetadata, doc.PublicMetadata);
            Assert.Equal(_fixture.Document.PrivateMetadata, doc.PrivateMetadata);
        }

        #endregion

        public Task InitializeAsync()
        {
            return Task.FromResult(0);
        }

        public async Task DisposeAsync()
        {
            // Restore document if it was deleted
            if (await IsDocumentDeletedAsync(_fixture.Document.Id))
                await RestoreArchivedDocumentAsync(_fixture.Document.Id);
        }
    }
}
