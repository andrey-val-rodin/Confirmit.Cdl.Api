using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Common;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class TrustedScopeTests : TestBase, IClassFixture<TrustedScopeFixture>
    {
        private readonly TrustedScopeFixture _fixture;

        public TrustedScopeTests(SharedFixture sharedFixture, TrustedScopeFixture fixture,
            ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region api.surveyrights

        [Fact]
        public async Task GetDocuments_NormalUserWithScopeApiSurveyrights_ResultContainsDataTemplate()
        {
            const string trustedScope = "cdl api.surveyrights";

            await Login(trustedScope);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{_fixture.Document.Name}'")).Items;

            Assert.Contains(documents, d => d.Id == _fixture.Document.Id);
        }

        [Fact]
        public async Task GetDocuments_NormalUserWithScopeApiSurveyrights_PermissionManage()
        {
            const string trustedScope = "cdl api.surveyrights";

            await Login(trustedScope);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{_fixture.Document.Name}'")).Items;

            Assert.Single(documents);
            Assert.Equal(_fixture.Document.Id, documents[0].Id);
            Assert.Equal(Permission.Manage, documents[0].Permission);
        }

        [Fact]
        public async Task GetDocumentPermission_NormalUserWithScopeApiSurveyrights_PermissionManage()
        {
            const string trustedScope = "cdl api.surveyrights";

            await Login(trustedScope);
            var permission = await GetPermissionAsync(_fixture.Document.Id);

            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task PatchDocument_NormalUserWithScopeApiSurveyrights_SuccessfullyUpdated()
        {
            const string trustedScope = "cdl api.surveyrights";

            await Login(trustedScope);
            await PatchDocumentAsync(_fixture.Document.Id, new DocumentPatchDto { SourceCode = "New source code " });
            var document = await GetDocumentAsync(_fixture.Document.Id);

            Assert.Equal("New source code ", document.SourceCode);
        }

        #endregion

        #region api.cdl.read

        [Fact]
        public async Task GetDocuments_NormalUserWithScopeApiCdlRead_ResultContainsDocument()
        {
            const string trustedScope = "cdl api.cdl.read";

            await Login(trustedScope);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{_fixture.Document.Name}'")).Items;

            Assert.Contains(documents, d => d.Id == _fixture.Document.Id);
        }

        [Fact]
        public async Task GetDocuments_NormalUserWithScopeApiCdlRead_PermissionView()
        {
            const string trustedScope = "cdl api.cdl.read";

            await Login(trustedScope);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{_fixture.Document.Name}'")).Items;

            Assert.Single(documents);
            Assert.Equal(_fixture.Document.Id, documents[0].Id);
            Assert.Equal(Permission.View, documents[0].Permission);
        }

        [Fact]
        public async Task GetDocumentPermission_NormalUserWithScopeApiCdlRead_PermissionView()
        {
            const string trustedScope = "cdl api.cdl.read";

            await Login(trustedScope);
            var permission = await GetPermissionAsync(_fixture.Document.Id);

            Assert.Equal(Permission.View, permission);
        }

        #endregion

        #region Helpers

        private async Task Login(string trustedScope)
        {
            var testClient = Scope.GetService<IConfirmitIntegrationTestsClient>();
            var token = await testClient.LoginUserAsync(
                NormalUser.Name, "password",
                scope: trustedScope);
            Assert.False(string.IsNullOrEmpty(token));

            Scope.GetService<IConfirmitTokenService>().SetToken(token);
        }

        #endregion
    }
}
