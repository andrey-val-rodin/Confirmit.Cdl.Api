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
    public class CompanyPermissionTests : TestBase, IClassFixture<CompanyPermissionFixture>
    {
        private readonly CompanyPermissionFixture _fixture;

        public CompanyPermissionTests(SharedFixture sharedFixture,
            CompanyPermissionFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GetPermission

        [Fact]
        public async Task GetPermission_NormalUser_Manage()
        {
            await UseNormalUserAsync();

            // NormalUser has permission "Manage" to the document because his company has permission "Manage"
            Assert.Equal(Permission.Manage, await GetPermissionAsync(_fixture.DocumentId));
        }

        #endregion

        #region GetAllCompanies

        [Fact]
        public async Task GetAllCompanies_SecondPage_ValidCompaniesAndLinks()
        {
            await UseAdminAsync();

            var page = await GetAllCompaniesAsync(_fixture.DocumentId, skip: 1, top: 1, orderBy: "Name desc");

            Assert.Equal(3, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(TestCompany.Name, item.Name);
            Assert.Equal(TestCompany.Id, item.Id);
            var expected = $"/api/cdl/documents/{_fixture.DocumentId}/companies?$skip=0&$top=1&$orderby=Name desc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
            expected = $"/api/cdl/documents/{_fixture.DocumentId}/companies?$skip=2&$top=1&$orderby=Name desc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
        }

        [Fact]
        public async Task GetAllCompanies_SearchByName_ValidCompaniesAndLinks()
        {
            await UseAdminAsync();

            var page = await GetAllCompaniesAsync(_fixture.DocumentId, skip: 1, top: 1, orderBy: "Name asc",
                filter: "startswith(Name, 'TestCompany')");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(TestCompany2.Name, item.Name);
            Assert.Equal(TestCompany2.Id, item.Id);
            var expected =
                $"/api/cdl/documents/{_fixture.DocumentId}/companies?$skip=0&$top=1&$orderby=Name asc&$filter=startswith(Name, 'TestCompany')";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
            Assert.Null(HttpUtility.UrlDecode(page.Links.NextPage));
        }

        #endregion

        #region GetCompanyPermissions

        [Fact]
        public async Task GetCompanyPermissions_FirstPage_ValidPermissionsAndLinks()
        {
            await UseAdminAsync();

            var page = await GetCompanyPermissionsAsync(_fixture.DocumentId, skip: 0, top: 1, orderBy: "Id asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(Admin.CompanyId, item.Id);
            Assert.Equal(Admin.Company.Name, item.Name);
            Assert.Equal(Permission.View, item.Permission);
            Assert.Null(page.Links.PreviousPage);
            var expected =
                $"/api/cdl/documents/{_fixture.DocumentId}/companypermissions?$skip=1&$top=1&$orderby=Id asc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
        }

        [Fact]
        public async Task GetCompanyPermissions_SearchByPermission_ValidResult()
        {
            await UseAdminAsync();

            var page = await GetCompanyPermissionsAsync(_fixture.DocumentId, skip: 0, top: 1,
                filter: "Permission eq 'Manage'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Null(page.Links.PreviousPage);
            Assert.Null(page.Links.NextPage);
            var item = page.Items[0];
            Assert.Equal(TestCompany.Id, item.Id);
            Assert.Equal(TestCompany.Name, item.Name);
            Assert.Equal(Permission.Manage, item.Permission);
        }

        #endregion

        #region PutCompanyPermissionAsync

        [Fact]
        public async Task PutCompanyPermissionAsync_NonExistentCompany_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = int.MaxValue, Permission = Permission.View },
                HttpStatusCode.NotFound, $"Company {int.MaxValue} not found");
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_WrongPermission_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionStub { Id = NormalUser.CompanyId, Permission = "WrongCode" },
                HttpStatusCode.BadRequest,
                "Error converting value \\\"WrongCode\\\" to type 'Confirmit.Cdl.Api.Authorization.Permission'.");
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_PermissionNone_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.None });
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_Twice_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_Twice_LastPermissionIsInEffect()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.Manage });

            var page = await GetCompanyPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.CompanyId}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Permission.Manage, page.Items[0].Permission);
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_PermissionNone_CompanyDeleted()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            var companies = (await GetCompanyPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.CompanyId}"))
                .Items;

            Assert.Single(companies);
            Assert.Equal(Permission.View, companies[0].Permission);

            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.None });
            companies = (await GetCompanyPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.CompanyId}")).Items;

            Assert.Empty(companies);
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_NormalUserWithoutAccessToCompany_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = Admin.CompanyId, Permission = Permission.View },
                HttpStatusCode.NotFound, $"Company {Admin.CompanyId} not found");
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_ProsUserHasAccessToCompany_Ok()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = Admin.CompanyId, Permission = Permission.View });
        }

        [Fact]
        public async Task PutCompanyPermissionAsync_NormalUserHasAccessToOwnCompany()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await UseNormalUserAsync();
            await GetDocumentAsync(document.Id);
        }

        #endregion

        #region DeleteCompanyPermission

        [Fact]
        public async Task DeleteDocumentCompanyPermission_NoPermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await DeleteCompanyPermissionAsync(document.Id, NormalUser.CompanyId);

            Assert.Empty((await GetCompanyPermissionsAsync(document.Id)).Items);
        }

        [Fact]
        public async Task DeleteDocumentCompanyPermission_NoCompany()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            Assert.Contains((await GetAllCompaniesAsync(document.Id)).Items, c => c.Id == NormalUser.CompanyId);

            await DeleteCompanyPermissionAsync(document.Id, NormalUser.CompanyId);

            Assert.DoesNotContain((await GetAllCompaniesAsync(document.Id)).Items, c => c.Id == NormalUser.CompanyId);
        }

        [Fact]
        public async Task DeleteDocumentCompanyPermission_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await UseNormalUserAsync();
            await DeleteCompanyPermissionAsync(document.Id, NormalUser.CompanyId, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteDocumentCompanyPermission_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await UseNormalUserAsync();
            await DeleteCompanyPermissionAsync(document.Id, NormalUser.CompanyId, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteDocumentCompanyPermission_NormalUserWithPermissionManageOnDocument_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage }
            });
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await UseNormalUserAsync();
            await DeleteCompanyPermissionAsync(document.Id, NormalUser.CompanyId);
        }

        [Fact]
        public async Task DeleteCompanyPermission_BothCompanyAndIndividualPermissions_IndividualPermissionStillExists()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage }
            });
            await PutCompanyPermissionAsync(document.Id,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await DeleteCompanyPermissionAsync(document.Id, NormalUser.CompanyId);

            Assert.Contains((await GetUserPermissionsAsync(document.Id)).Items, p => p.Id == NormalUser.Id);
        }

        [Fact]
        public async Task DeleteCompanyPermission_CompanyDoesNotExist_NotFound()
        {
            await UseNormalUserAsync();

            await DeleteCompanyPermissionAsync(_fixture.DocumentId, 0,
                HttpStatusCode.NotFound, "Company 0 not found");
        }

        #endregion
    }
}
