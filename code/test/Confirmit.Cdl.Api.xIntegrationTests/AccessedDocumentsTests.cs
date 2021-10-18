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
    public class AccessedDocumentsTests : TestBase, IClassFixture<AccessedDocumentFixture>
    {
        private readonly AccessedDocumentFixture _fixture;

        public AccessedDocumentsTests(SharedFixture sharedFixture, AccessedDocumentFixture fixture,
            ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GetAccessedDocuments

        [Fact]
        public async Task GetAccessedDocuments_User_CorrectPage()
        {
            await UseNormalUserAsync();
            var page = await GetAccessedDocumentsAsync(skip: 1, top: 1, filter: $"Name eq '{Prefix + _fixture.Name}'");

            Assert.True(5 == page.TotalCount, "NormalUser has access to 5 documents");
            Assert.Single(page.Items);
            var expected = $"/api/cdl/documents/accessed?$skip=0&$top=1&$filter=Name eq '{Prefix + _fixture.Name}'";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
            expected = $"/api/cdl/documents/accessed?$skip=2&$top=1&$filter=Name eq '{Prefix + _fixture.Name}'";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
        }

        [Fact]
        public async Task GetAccessedDocuments_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await GetAccessedDocumentsAsync(null, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        #endregion

        #region Accessed revisions

        [Fact]
        public async Task GetAccessedRevisions_Enduser_CorrectPage()
        {
            await UseEnduserAsync();
            var page = await GetAccessedRevisionsAsync(filter: $"Name eq '{Prefix + _fixture.Name}'");

            Assert.True(5 == page.TotalCount, "Enduser has access to 5 revisions");
            Assert.Equal(5, page.Items.Count);
            Assert.Null(page.Links.PreviousPage);
            Assert.Null(page.Links.NextPage);
        }

        #endregion
    }
}
