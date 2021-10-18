using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using JetBrains.Annotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class CommitTests : TestBase, IClassFixture<CommitFixture>
    {
        private readonly CommitFixture _fixture;

        public CommitTests(SharedFixture sharedFixture, CommitFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetDocumentCommits_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await GetCommitsAsync(_fixture.DocumentId, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDocumentCommits_UserWithPermissionView_Forbidden()
        {
            await UseCompanyAdminAsync();
            await GetCommitsAsync(_fixture.DocumentId, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDocumentCommits_UserWithPermissionManage_Ok()
        {
            await UseNormalUserAsync();
            var page = await GetCommitsAsync(_fixture.DocumentId);

            Assert.Equal(_fixture.ExpectedCommits.Count, page.TotalCount);
            Assert.Equal(_fixture.ExpectedCommits.Count, page.Items.Count);
        }

        [Fact]
        public async Task GetDocumentCommits_Ascending_CorrectOrder()
        {
            await UseNormalUserAsync();
            var commits = (await GetCommitsAsync(_fixture.DocumentId)).Items;

            AssertCommit(_fixture.ExpectedCommits.FirstOrDefault(), commits[0]);
        }

        [Fact]
        public async Task GetDocumentCommits_Descending_CorrectOrder()
        {
            await UseNormalUserAsync();
            var commits = (await GetCommitsAsync(_fixture.DocumentId, orderBy: "Id desc")).Items;

            AssertCommit(_fixture.ExpectedCommits.LastOrDefault(), commits[0]);
        }

        [Fact]
        public async Task GetDocumentCommits_Page_CorrectLinks()
        {
            await UseNormalUserAsync();
            var page = await GetCommitsAsync(_fixture.DocumentId, skip: 1, top: 1, orderBy: "Id desc");

            var expected = $"/api/cdl/documents/{_fixture.DocumentId}/commits?$skip=0&$top=1&$orderby=Id desc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
            expected = $"/api/cdl/documents/{_fixture.DocumentId}/commits?$skip=2&$top=1&$orderby=Id desc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
        }

        [Fact]
        public async Task GetDocumentCommits_AllCommits_AreValid()
        {
            await UseNormalUserAsync();
            for (int i = 0; i < _fixture.ExpectedCommits.Count; i++)
            {
                AssertCommit(_fixture.ExpectedCommits[i], await GetCommitAsync(i));
            }
        }


        #region Helpers

        private async Task<CommitDto> GetCommitAsync(int index)
        {
            Assert.True(index >= 0);

            var page = await GetCommitsAsync(_fixture.DocumentId, skip: index, top: 1, orderBy: "Id asc");

            Assert.Single(page.Items);

            return page.Items[0];
        }

        [AssertionMethod]
        private static void AssertCommit(CommitDto expected, CommitDto actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);

            Assert.Equal(expected.DocumentId, actual.DocumentId);
            Assert.Equal(expected.RevisionId, actual.RevisionId);
            Assert.Equal(expected.RevisionNumber, actual.RevisionNumber);
            Assert.Equal(expected.Action, actual.Action);
            Assert.Equal(expected.CreatedBy, actual.CreatedBy);
            if (expected.Revision == null)
                Assert.Null(actual.Revision);
            else
                Assert.Equal(expected.Revision.CreatedBy, actual.Revision.CreatedBy);
        }

        #endregion
    }
}
