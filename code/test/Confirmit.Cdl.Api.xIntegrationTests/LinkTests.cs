using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class LinkTests : TestBase, IClassFixture<LinkFixture>
    {
        private readonly LinkFixture _fixture;

        public LinkTests(SharedFixture sharedFixture, LinkFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GET Root

        [Fact]
        public async Task GetRoot_Unauthorized_ValidLinks()
        {
            UseUnauthorizedUser();
            var root = await GetRootAsync(HttpStatusCode.Unauthorized);

            // Unauthorized user should see limited set of links
            const string expected =
                @"{
  ""self"": ""/api/cdl"",
  ""spec"": ""/api/cdl/swagger""
}";
            AssertValidLinks(expected, root.Links.ToString());
        }

        [Fact]
        public async Task GetRoot_User_ValidLinks()
        {
            await UseNormalUserAsync();
            var root = await GetRootAsync();

            // User with permission View should see all links
            const string expected =
                @"{
  ""self"": ""/api/cdl"",
  ""documents"": ""/api/cdl/documents"",
  ""publishedRevisions"": ""/api/cdl/documents/revisions/published"",
  ""spec"": ""/api/cdl/swagger""
}";
            AssertValidLinks(expected, root.Links.ToString());
        }

        [Fact]
        public async Task GetRoot_Enduser_ValidLinks()
        {
            await UseEnduserAsync();
            var root = await GetRootAsync();

            // Enduser cannot see link to documents because he has access to published revisions only
            const string expected =
                @"{
  ""self"": ""/api/cdl"",
  ""publishedRevisions"": ""/api/cdl/documents/revisions/published"",
  ""spec"": ""/api/cdl/swagger""
}";
            AssertValidLinks(expected, root.Links.ToString());
        }

        #endregion

        #region GET Documents

        [Fact]
        public async Task GetDocuments_TakeAndSkipParametersAreSpecified_ReturnsValidLinks()
        {
            await UseAdminAsync();

            var page = await GetDocumentsAsync(skip: 1, top: 1);

            Assert.True(page.TotalCount >= 3);
            AssertValidLinks("/api/cdl/documents?$skip=0&$top=1", page.Links.PreviousPage);
            AssertValidLinks("/api/cdl/documents?$skip=2&$top=1", page.Links.NextPage);
        }

        [Fact]
        public async Task GetDocuments_ManySearchFields_ReturnsValidLinks()
        {
            await UseAdminAsync();

            var filter =
                $"startswith(CompanyName, '{Prefix + _fixture.Name}') or startswith(Name, '{Prefix + _fixture.Name}')";
            var page = await GetDocumentsAsync(skip: 1, top: 1, filter: filter);

            Assert.Equal(3, page.TotalCount);
            AssertValidLinks("/api/cdl/documents?$skip=0&$top=1&$filter=" + filter,
                HttpUtility.UrlDecode(page.Links.PreviousPage));
            AssertValidLinks("/api/cdl/documents?$skip=2&$top=1&$filter=" + filter,
                HttpUtility.UrlDecode(page.Links.NextPage));
        }

        #endregion

        #region GET AccessedDocuments

        [Fact]
        public async Task GetAccessedDocuments_ManySearchFields_ValidLinks()
        {
            await UseAdminAsync();

            var filter =
                $"startswith(CompanyName, '{Prefix + _fixture.Name}') or startswith(Name, '{Prefix + _fixture.Name}')";
            var page = await GetAccessedDocumentsAsync(skip: 1, top: 1, filter: filter);

            Assert.Equal(3, page.TotalCount);
            AssertValidLinks("/api/cdl/documents/accessed?$skip=0&$top=1&$filter=" + filter,
                HttpUtility.UrlDecode(page.Links.PreviousPage));
            AssertValidLinks("/api/cdl/documents/accessed?$skip=2&$top=1&$filter=" + filter,
                HttpUtility.UrlDecode(page.Links.NextPage));
        }

        #endregion

        #region GET GetArchivedDocuments

        [Fact]
        public async Task GetArchivedDocuments_ManySearchFields_ValidLinks()
        {
            await UseAdminAsync();

            var filter =
                $"startswith(CompanyName, '{Prefix + _fixture.Name}') or startswith(Name, '{Prefix + _fixture.Name}')";
            var page = await GetArchivedDocumentsAsync(skip: 0, top: 1, filter: filter);

            Assert.True(page.TotalCount == 2);
            Assert.Null(page.Links.PreviousPage);
            AssertValidLinks("/api/cdl/documents/deleted?$skip=1&$top=1&$filter=" + filter,
                HttpUtility.UrlDecode(page.Links.NextPage));
        }

        #endregion

        #region GET document by alias

        [Fact]
        public async Task GetDocumentByAlias_FullDocument_ValidLinks()
        {
            await UseAdminAsync();
            var document = await GetDocumentByAliasAsync(_fixture.Alias.Namespace, _fixture.Alias.Alias);

            // Document with aliases, commits and revisions should have all links
            var expected =
                $@"{{
  ""aliases"": ""/api/cdl/documents/aliases?$filter=DocumentId eq {_fixture.DocumentId}"",
  ""commits"": ""/api/cdl/documents/{_fixture.DocumentId}/commits"",
  ""revisions"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions"",
  ""publishedRevision"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions/published""
}}";
            AssertValidLinks(expected, HttpUtility.UrlDecode(document.Links.ToString()));
        }

        [Fact]
        public async Task GetDocumentByAlias_UserWithPermissionView_ValidLinks()
        {
            await UseNormalUserAsync();
            var document = await GetDocumentByAliasAsync(_fixture.Alias.Namespace, _fixture.Alias.Alias);

            // Viewer should see aliases and published revisions only
            var expected =
                $@"{{
  ""aliases"": ""/api/cdl/documents/aliases?$filter=DocumentId eq {_fixture.DocumentId}"",
  ""publishedRevision"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions/published""
}}";
            AssertValidLinks(expected, HttpUtility.UrlDecode(document.Links.ToString()));
        }

        #endregion

        #region GET document by Id

        [Fact]
        public async Task GetDocumentById_FullDocument_ValidLinks()
        {
            await UseAdminAsync();
            var document = await GetDocumentAsync(_fixture.DocumentId);

            // Document with aliases, commits and revisions should have all links
            var expected =
                $@"{{
  ""aliases"": ""/api/cdl/documents/aliases?$filter=DocumentId eq {_fixture.DocumentId}"",
  ""commits"": ""/api/cdl/documents/{_fixture.DocumentId}/commits"",
  ""revisions"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions"",
  ""publishedRevision"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions/published""
}}";
            AssertValidLinks(expected, HttpUtility.UrlDecode(document.Links.ToString()));
        }

        [Fact]
        public async Task GetDocumentById_UserWithPermissionView_ValidLinks()
        {
            await UseNormalUserAsync();
            var document = await GetDocumentAsync(_fixture.DocumentId);

            // Viewer should see aliases and published revisions only
            var expected =
                $@"{{
  ""aliases"": ""/api/cdl/documents/aliases?$filter=DocumentId eq {_fixture.DocumentId}"",
  ""publishedRevision"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions/published""
}}";
            AssertValidLinks(expected, HttpUtility.UrlDecode(document.Links.ToString()));
        }

        [Fact]
        public async Task GetDocumentById_BareDocument_ValidLinks()
        {
            await UseAdminAsync();
            var document = await GetDocumentAsync(_fixture.BareDocumentId);

            // Document without aliases, commits and revisions should not have links
            Assert.Null(document.Links);
        }

        #endregion

        #region PATCH document

        [Fact]
        public async Task PatchDocument_FullDocument_ValidLinks()
        {
            await UseAdminAsync();
            var document = await PatchDocumentAsync(_fixture.DocumentId, new DocumentPatchDto());

            // Document with aliases, commits and revisions should have all links
            var expected =
                $@"{{
  ""aliases"": ""/api/cdl/documents/aliases?$filter=DocumentId eq {_fixture.DocumentId}"",
  ""commits"": ""/api/cdl/documents/{_fixture.DocumentId}/commits"",
  ""revisions"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions"",
  ""publishedRevision"": ""/api/cdl/documents/{_fixture.DocumentId}/revisions/published""
}}";
            AssertValidLinks(expected, HttpUtility.UrlDecode(document.Links.ToString()));
        }

        [Fact]
        public async Task PatchDocument_BareDocument_ValidLinks()
        {
            await UseAdminAsync();
            var document = await PatchDocumentAsync(_fixture.BareDocumentId, new DocumentPatchDto());

            // Document without aliases, commits and revisions should not have links
            Assert.Null(document.Links);
        }

        #endregion

        #region DELETE document

        [Fact]
        public async Task DeleteDocument_ValidLinks()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var links = await DeleteDocumentAsync(id);

            // Document with aliases, commits and revisions should have all links
            var expected =
                $@"{{
  ""links"": {{
    ""archive"": ""/api/cdl/documents/{id}/deleted""
  }}
}}";
            AssertValidLinks(expected, links.ToString());
        }

        #endregion

        #region GET commits

        [Fact]
        public async Task GetCommits_FirstCommit_ValidLinks()
        {
            await UseAdminAsync();
            var commit = (await GetCommitsAsync(_fixture.DocumentId)).Items[0];

            // First commit should have link to revision
            var expected =
                $@"{{
  ""revision"": ""/api/cdl/documents/revisions/{_fixture.RevisionId}""
}}";
            AssertValidLinks(expected, commit.Links.ToString());
        }

        [Fact]
        public async Task GetCommits_LastCommit_ValidLinks()
        {
            await UseAdminAsync();
            var commit = (await GetCommitsAsync(_fixture.DocumentId)).Items[2];

            // Commit should not have link to revision because it was deleted
            Assert.Null(commit.Links);
        }

        #endregion

        #region GET Revisions

        [Fact]
        public async Task GetRevisions_ManySearchFields_ValidLinks()
        {
            await UseAdminAsync();

            var page = await GetRevisionsAsync(_fixture.DocumentId, skip: 0, top: 1);

            Assert.Equal(1, page.TotalCount);
            Assert.Null(page.Links.PreviousPage);
            Assert.Null(page.Links.NextPage);
        }

        #endregion

        #region POST revision

        [Fact]
        public async Task PostRevision_ValidLinks()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var revision = await PostRevisionAsync(id);

            // Revision should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{id}""
}}";
            AssertValidLinks(expected, revision.Links.ToString());
        }

        #endregion

        #region GET published revision

        [Fact]
        public async Task GetPublishedRevision_ValidLinks()
        {
            await UseNormalUserAsync();
            var revision = await GetPublishedRevisionAsync(_fixture.DocumentId);

            // Revision should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{_fixture.DocumentId}""
}}";
            AssertValidLinks(expected, revision.Links.ToString());
        }

        #endregion

        #region GET revision by Id

        [Fact]
        public async Task GetRevision_ValidLinks()
        {
            await UseAdminAsync();
            var revision = await GetRevisionAsync(_fixture.RevisionId);

            // Revision should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{_fixture.DocumentId}""
}}";
            AssertValidLinks(expected, revision.Links.ToString());
        }

        #endregion

        #region POST alias

        [Fact]
        public async Task PostAlias_ValidLinks()
        {
            await UseAdminAsync();
            var alias = await PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "LinksTests",
                Alias = "PostAlias_ValidLinks",
                DocumentId = _fixture.DocumentId
            });

            // Alias should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{_fixture.DocumentId}""
}}";
            AssertValidLinks(expected, alias.Links.ToString());
        }

        #endregion

        #region PATCH alias

        [Fact]
        public async Task PatchAlias_ValidLinks()
        {
            await UseAdminAsync();
            var alias = await PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "LinksTests",
                Alias = "PatchAlias_ValidLinks",
                DocumentId = _fixture.DocumentId
            });
            var id = (await PostDocumentAsync()).Id;
            alias = await PatchAliasAsync(alias.Id, new AliasPatchDto { DocumentId = id });

            // Alias should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{id}""
}}";
            AssertValidLinks(expected, alias.Links.ToString());
        }

        #endregion

        #region GET alias by Id

        [Fact]
        public async Task GetAlias_ValidLinks()
        {
            await UseNormalUserAsync();
            var alias = await GetAliasAsync(_fixture.Alias.Id);

            // Alias should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{_fixture.DocumentId}""
}}";
            AssertValidLinks(expected, alias.Links.ToString());
        }

        #endregion

        #region GET aliases

        [Fact]
        public async Task GetAliases_FirstAlias_ValidLinks()
        {
            await UseAdminAsync();
            var alias = (await GetAliasesAsync(filter: $"DocumentId eq {_fixture.DocumentId}")).Items[0];

            // Alias should have link to document
            var expected =
                $@"{{
  ""document"": ""/api/cdl/documents/{_fixture.DocumentId}""
}}";
            AssertValidLinks(expected, alias.Links.ToString());
        }

        #endregion
    }
}
