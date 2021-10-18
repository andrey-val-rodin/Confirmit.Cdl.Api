using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class AliasTests : TestBase, IClassFixture<AliasFixture>
    {
        private readonly AliasFixture _fixture;

        public AliasTests(SharedFixture sharedFixture, AliasFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GetAlias

        [Fact]
        public async Task GetAlias_NormalUser_Ok()
        {
            await UseNormalUserAsync();

            Assert.Equal(
                Newtonsoft.Json.JsonConvert.SerializeObject(_fixture.Alias),
                Newtonsoft.Json.JsonConvert.SerializeObject(await GetAliasAsync(_fixture.Alias.Id)));
        }

        [Fact]
        public async Task GetAlias_EndUser_Ok()
        {
            await UseEnduserAsync();

            Assert.Equal(
                Newtonsoft.Json.JsonConvert.SerializeObject(_fixture.Alias),
                Newtonsoft.Json.JsonConvert.SerializeObject(await GetAliasAsync(_fixture.Alias.Id)));
        }

        #endregion

        #region PostAlias

        [Fact]
        public async Task PostAlias_WrongNamespace_BadRequest()
        {
            await UseAdminAsync();
            var alias = new AliasToCreateDto
                { Namespace = "namespace?", Alias = "alias", DocumentId = _fixture.DocumentId };
            await PostAliasAsync(alias, HttpStatusCode.BadRequest, "Field namespace is not well-formed URI part.");
        }

        [Fact]
        public async Task PostAlias_Uppercase_ConvertedToLowercase()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            var alias = new AliasToCreateDto { Namespace = "NAMESPACE", Alias = "ALIAS", DocumentId = id };
            var newAlias = await PostAliasAsync(alias);

            Assert.Equal("namespace", newAlias.Namespace);
            Assert.Equal("alias", newAlias.Alias);
        }

        [Fact]
        public async Task PostAlias_AliasDuplication_BadRequest()
        {
            await UseAdminAsync();
            var alias = new AliasToCreateDto
            {
                Namespace = _fixture.Alias.Namespace,
                Alias = _fixture.Alias.Alias,
                DocumentId = _fixture.DocumentId
            };
            await PostAliasAsync(alias, HttpStatusCode.BadRequest,
                "Unable to set new alias. Try different namespace or alias name.");
        }

        [Fact]
        public async Task PostAlias_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            var alias = new AliasToCreateDto
                { Namespace = "namespace", Alias = "alias", DocumentId = _fixture.DocumentId };
            await PostAliasAsync(alias, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostAlias_UserWithPermissionView_Forbidden()
        {
            await UseNormalUserAsync();
            var alias = new AliasToCreateDto
                { Namespace = "namespace", Alias = "alias", DocumentId = _fixture.DocumentId };
            await PostAliasAsync(alias, HttpStatusCode.Forbidden);
        }

        #endregion

        #region PatchAlias

        [Fact]
        public async Task PatchAlias_UserHasPermissionViewOnOldDocument_Forbidden()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchAliasAsync(_fixture.Alias.Id, new AliasPatchDto { DocumentId = id }, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchAlias_UserHasPermissionViewOnNewDocument_Forbidden()
        {
            try
            {
                // Set permission "Manage" to old document for normal user
                await UseAdminAsync();
                await PatchUserPermissionsAsync(_fixture.DocumentId,
                    new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

                // Set permission "View" to new document for normal user
                var id = (await PostDocumentAsync()).Id;
                await PatchUserPermissionsAsync(_fixture.DocumentId,
                    new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

                await UseNormalUserAsync();
                await PatchAliasAsync(_fixture.Alias.Id, new AliasPatchDto { DocumentId = id },
                    HttpStatusCode.Forbidden);
            }
            finally
            {
                // Restore permission of normal user
                await UseAdminAsync();
                await PatchUserPermissionsAsync(_fixture.DocumentId,
                    new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });
            }
        }

        [Fact]
        public async Task PatchAlias_ChangeLinkToNewDocument_Ok()
        {
            try
            {
                await UseAdminAsync();
                var id = (await PostDocumentAsync()).Id;
                var newAlias = await PatchAliasAsync(_fixture.Alias.Id, new AliasPatchDto { DocumentId = id });

                Assert.Equal(_fixture.Alias.Id, newAlias.Id);
                Assert.Equal(_fixture.Alias.Namespace, newAlias.Namespace);
                Assert.Equal(_fixture.Alias.Alias, newAlias.Alias);
                Assert.Equal(id, newAlias.DocumentId);
            }
            finally
            {
                // Restore link
                await PatchAliasAsync(_fixture.Alias.Id, new AliasPatchDto { DocumentId = _fixture.DocumentId });
            }
        }

        [Fact]
        public async Task PatchAlias_TheSameLink_Ok()
        {
            await UseAdminAsync();
            await PatchAliasAsync(_fixture.Alias.Id, new AliasPatchDto { DocumentId = _fixture.DocumentId });
        }

        #endregion

        #region GetAliases

        [Fact]
        public async Task GetAliases_NormalUser_AliasOfOneDocumentOnly()
        {
            await UseNormalUserAsync();
            var page = await GetAliasesAsync(filter: "Namespace eq 'aliases-tests'");

            // Normal user has access to document with Id = _documentId only
            // Therefore, he obtains one alias only
            Assert.Equal(1, page.TotalCount);
            Assert.Equal(_fixture.DocumentId, page.Items[0].DocumentId);
        }

        [Fact]
        public async Task GetAliases_Admin_ValidLinks()
        {
            await UseAdminAsync();
            var page = await GetAliasesAsync(top: 2, skip: 1);

            Assert.Contains("documents/aliases?$skip=0&$top=2", page.Links.PreviousPage); // skip = 0
            Assert.Contains("documents/aliases?$skip=3&$top=2", page.Links.NextPage); // skip = 3
        }

        [Fact]
        public async Task GetAliases_SearchById_AvailableAliases()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: $"Id eq {_fixture.Alias.Id}")).Items;

            Assert.Single(aliases);
            Assert.Equal(_fixture.Alias.Id, aliases[0].Id);
        }

        [Fact]
        public async Task GetAliases_SearchByDocumentId_AvailableAliases()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: $"DocumentId eq {_fixture.DocumentWith2AliasesId}")).Items;

            Assert.Equal(2, aliases.Count);
        }

        #endregion

        #region GetDocumentByAlias

        [Fact]
        public async Task GetDocumentByAlias_NonexistentAlias_NotFound()
        {
            await UseAdminAsync();
            await GetDocumentByAliasAsync("aliases-tests", "nonexistent", HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDocumentByAlias_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await GetDocumentByAliasAsync(_fixture.Alias.Namespace, _fixture.Alias.Alias, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDocumentByAlias_User_Ok()
        {
            await UseNormalUserAsync();
            Assert.Equal(_fixture.DocumentId,
                (await GetDocumentByAliasAsync(_fixture.Alias.Namespace, _fixture.Alias.Alias)).Id);
        }

        #endregion

        #region GetPublishedRevisionByAlias

        [Fact]
        public async Task GetPublishedRevisionByAlias_NonexistentAlias_NotFound()
        {
            await UseAdminAsync();
            await GetPublishedRevisionByAliasAsync("aliases-tests", "nonexistent", HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPublishedRevisionByAlias_Enduser_Ok()
        {
            await UseEnduserAsync();
            Assert.Equal(_fixture.DocumentId,
                (await GetPublishedRevisionByAliasAsync(_fixture.Alias.Namespace, _fixture.Alias.Alias)).DocumentId);
        }

        [Fact]
        public async Task GetPublishedRevisionByAlias_User_Ok()
        {
            await UseNormalUserAsync();
            Assert.Equal(_fixture.DocumentId,
                (await GetPublishedRevisionByAliasAsync(_fixture.Alias.Namespace, _fixture.Alias.Alias)).DocumentId);
        }

        #endregion
    }
}
