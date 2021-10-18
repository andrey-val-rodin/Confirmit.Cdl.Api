using Confirmit.Cdl.Api.Authorization;
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
    public class EnduserListPermissionTests : TestBase, IClassFixture<EnduserListPermissionFixture>
    {
        private readonly EnduserListPermissionFixture _fixture;

        public EnduserListPermissionTests(SharedFixture sharedFixture,
            EnduserListPermissionFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GetPermission

        [Fact]
        public async Task GetPermission_Enduser_View()
        {
            await UseEnduserAsync();

            // Enduser has access to the document because his enduser list has permission "View"
            Assert.Equal(Permission.View, await GetPermissionAsync(_fixture.DocumentId));
        }

        #endregion

        #region GetAllEnduserLists

        [Fact]
        public async Task GetAllEnduserLists_SecondPage_ValidEnduserListsAndLinks()
        {
            await UseAdminAsync();

            var page = await GetAllEnduserListsAsync(_fixture.DocumentId, skip: 1, top: 1, orderBy: "Name asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(EnduserList2.Name, item.Name);
            Assert.Equal(EnduserList2.Id, item.Id);
            var expected = $"/api/cdl/documents/{_fixture.DocumentId}/enduserlists?$skip=0&$top=1&$orderby=Name asc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
            Assert.Null(page.Links.NextPage);
        }

        [Fact]
        public async Task GetAllEnduserLists_SearchByName_ValidEnduserListsAndLinks()
        {
            await UseAdminAsync();

            var page = await GetAllEnduserListsAsync(_fixture.DocumentId, skip: 1, top: 1, orderBy: "Name asc",
                filter: "startswith(Name, 'EnduserList')");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(EnduserList2.Name, item.Name);
            Assert.Equal(EnduserList2.Id, item.Id);
            var expected =
                $"/api/cdl/documents/{_fixture.DocumentId}/enduserlists?$skip=0&$top=1&$orderby=Name asc&$filter=startswith(Name, 'EnduserList')";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
            Assert.Null(HttpUtility.UrlDecode(page.Links.NextPage));
        }

        #endregion

        #region GetEnduserListPermissions

        [Fact]
        public async Task GetEnduserListPermissions_FirstPage_ValidPermissionsAndLinks()
        {
            await UseAdminAsync();

            var page = await GetEnduserListPermissionsAsync(_fixture.DocumentId, skip: 0, top: 1, orderBy: "Id asc");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(EnduserList.Id, item.Id);
            Assert.Equal(EnduserList.Name, item.Name);
            Assert.Equal(Permission.View, item.Permission);
            Assert.Null(page.Links.PreviousPage);
            Assert.Null(page.Links.NextPage);
        }

        [Fact]
        public async Task GetEnduserListPermissions_SearchByPermission_ValidResult()
        {
            await UseAdminAsync();

            var page = await GetEnduserListPermissionsAsync(_fixture.DocumentId, skip: 0, top: 1,
                filter: "Permission eq 'View'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Null(page.Links.PreviousPage);
            Assert.Null(page.Links.NextPage);
            var item = page.Items[0];
            Assert.Equal(EnduserList.Id, item.Id);
            Assert.Equal(EnduserList.Name, item.Name);
            Assert.Equal(Permission.View, item.Permission);
        }

        #endregion

        #region PutEnduserListPermissionAsync

        [Fact]
        public async Task PutEnduserListPermissionAsync_NonExistentEnduserList_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = int.MaxValue, Permission = Permission.View },
                HttpStatusCode.NotFound, $"Enduser list {int.MaxValue} not found");
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_WrongPermission_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionStub { Id = EnduserList.Id, Permission = "WrongCode" },
                HttpStatusCode.BadRequest,
                "Error converting value \\\"WrongCode\\\" to type 'Confirmit.Cdl.Api.Authorization.Permission'.");
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_PermissionNone_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.None });
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_Twice_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.None });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.None });
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_Twice_LastPermissionIsInEffect()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.None });

            var page = await GetEnduserListPermissionsAsync(document.Id);

            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_PermissionNone_EnduserListDeleted()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            var enduserLists = (await GetEnduserListPermissionsAsync(document.Id)).Items;

            Assert.Single(enduserLists);
            Assert.Equal(Permission.View, enduserLists[0].Permission);

            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.None });
            enduserLists = (await GetEnduserListPermissionsAsync(document.Id)).Items;

            Assert.Empty(enduserLists);
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_NormalUserWithoutAccessToEnduserList_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View },
                HttpStatusCode.NotFound, $"Enduser list {EnduserList.Id} not found");
        }

        [Fact]
        public async Task PutEnduserListPermissionAsync_ProsUserHasAccessToEnduserList_Ok()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });
        }

        #endregion

        #region DeleteEnduserListPermission

        [Fact]
        public async Task DeleteDocumentEnduserListPermission_NoPermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await DeleteEnduserListPermissionAsync(document.Id, EnduserList.Id);

            Assert.Empty((await GetEnduserListPermissionsAsync(document.Id)).Items);
        }

        [Fact]
        public async Task DeleteDocumentEnduserListPermission_NoEnduserList()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            Assert.Contains((await GetAllEnduserListsAsync(document.Id)).Items, c => c.Id == EnduserList.Id);

            await DeleteEnduserListPermissionAsync(document.Id, EnduserList.Id);

            Assert.DoesNotContain((await GetAllEnduserListsAsync(document.Id)).Items, c => c.Id == EnduserList.Id);
        }

        [Fact]
        public async Task DeleteDocumentEnduserListPermission_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await UseNormalUserAsync();
            await DeleteEnduserListPermissionAsync(document.Id, EnduserList.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteDocumentEnduserListPermission_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await UseNormalUserAsync();
            await DeleteEnduserListPermissionAsync(document.Id, EnduserList.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteDocumentEnduserListPermission_NormalUserWithPermissionManageOnDocument_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage }
            });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await UseNormalUserAsync();
            await DeleteEnduserListPermissionAsync(document.Id, EnduserList.Id);
        }

        [Fact]
        public async Task DeleteEnduserListPermission_BothEnduserListAndIndividualPermissions_IndividualPermissionStillExists()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await DeleteEnduserListPermissionAsync(document.Id, EnduserList.Id);

            Assert.Contains((await GetEnduserPermissionsAsync(document.Id)).Items, p => p.Id == Enduser.Id);
        }

        [Fact]
        public async Task DeleteEnduserListPermission_EnduserListDoesNotExist_NotFound()
        {
            await UseAdminAsync();

            await DeleteEnduserListPermissionAsync(_fixture.DocumentId, 0,
                HttpStatusCode.NotFound, "Enduser list 0 not found");
        }

        #endregion
    }
}
