using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.xUnitTests.Fixtures;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xUnitTests
{
    public class CustomerTests : IClassFixture<Fixture>
    {
        private readonly Fixture _fixture;

        public CustomerTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        #region Admin

        [Fact]
        public async Task CreateCustomer_Admin_DocumentQueryContainsAllDocuments()
        {
            var customer = await CreateCustomerAsync(AdminPrincipal);

            Assert.Equal(Context.Documents.Count(), customer.DocumentAccessor.GetQuery().ToList().Count);
        }

        [Fact]
        public async Task CreateCustomer_AdminAndStatusExists_HasAllPermissionsForAllExistentDocuments()
        {
            var customer = await CreateCustomerAsync(AdminPrincipal);

            foreach (var document in Context.Documents.Where(d => d.Deleted == null))
            {
                Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                    document.Id, Permission.View, ResourceStatus.Exists));
                Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                    document.Id, Permission.Manage, ResourceStatus.Exists));
            }
        }

        [Fact]
        public async Task CreateCustomer_AdminAndStatusArchived_HasAllPermissionsForAllArchivedDocuments()
        {
            var customer = await CreateCustomerAsync(AdminPrincipal);

            foreach (var document in Context.Documents.Where(d => d.Deleted != null))
            {
                Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                    document.Id, Permission.View, ResourceStatus.Archived));
                Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                    document.Id, Permission.Manage, ResourceStatus.Archived));
            }
        }

        [Fact]
        public async Task CreateCustomer_AdminAndStatusExistsAndNonexistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(AdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(long.MaxValue, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.View, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_AdminAndStatusArchivedAndNonexistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(AdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(long.MaxValue, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.Manage, ResourceStatus.Archived));
        }

        #endregion

        #region ProS

        [Fact]
        public async Task CreateCustomer_ProsUser_DocumentQueryContainsAllDocuments()
        {
            var customer = await CreateCustomerAsync(ProsPrincipal);

            Assert.Equal(Context.Documents.Count(), customer.DocumentAccessor.GetQuery().ToList().Count);
        }

        [Fact]
        public async Task CreateCustomer_ProsUserAndStatusExists_HasPermissionManageForAllExistentDocuments()
        {
            var customer = await CreateCustomerAsync(ProsPrincipal);

            foreach (var document in Context.Documents.Where(d => d.Deleted == null))
            {
                Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                    document.Id, Permission.Manage, ResourceStatus.Exists));
            }
        }

        [Fact]
        public async Task CreateCustomer_ProsUserAndStatusArchived_HasPermissionManageForAllArchivedDocuments()
        {
            var customer = await CreateCustomerAsync(ProsPrincipal);

            foreach (var document in Context.Documents.Where(d => d.Deleted != null))
            {
                Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                    document.Id, Permission.Manage, ResourceStatus.Archived));
            }
        }

        [Fact]
        public async Task CreateCustomer_ProsUserAndStatusExistsAndNonexistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(ProsPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(long.MaxValue, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.View, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ProsUserAndStatusArchivedAndNonexistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(ProsPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(long.MaxValue, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.Manage, ResourceStatus.Archived));
        }

        #endregion

        #region CompanyAdmin

        [Fact]
        public async Task CreateCustomer_CompanyAdmin_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);
            var query = customer.DocumentAccessor.GetQuery();

            Assert.Equal(4, query.ToList().Count);
            Assert.True(query.Any(d => d.Resource.Id == 1));
            Assert.True(query.Any(d => d.Resource.Id == 2));
            Assert.True(query.Any(d => d.Resource.Id == 3));
            Assert.True(query.Any(d => d.Resource.Id == 6));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusExistsExist_HasAllPermissionsForDocument1()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                1, Permission.View, ResourceStatus.Exists));
            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                1, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusArchived_ExceptionForDocument1()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(1, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(1, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusExist_HasAllPermissionsForDocument2()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                2, Permission.View, ResourceStatus.Exists));
            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                2, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusArchived_ExceptionForDocument2()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(2, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(2, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusExists_HasPermissionManageForDocument3()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                3, Permission.View, ResourceStatus.Exists));
            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                3, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusArchived_ExceptionForDocument3()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(3, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(3, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusExists_HasNotPermissionsForDocument4()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                4, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                4, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusArchived_ExceptionForDocument4()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(4, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(4, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusExists_ExceptionForDocument6()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(6, Permission.View, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(6, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_CompanyAdminAndStatusArchived_HasPermissionManageForDocument6()
        {
            var customer = await CreateCustomerAsync(CompanyAdminPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                6, Permission.View, ResourceStatus.Archived));
            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                6, Permission.Manage, ResourceStatus.Archived));
        }

        #endregion

        #region ProsCompany

        [Fact]
        public async Task CreateCustomer_ProsCompany_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(ProsCompanyPrincipal);
            var query = customer.DocumentAccessor.GetQuery();

            Assert.Equal(4, query.ToList().Count);
            Assert.True(query.Any(d => d.Resource.Id == 1));
            Assert.True(query.Any(d => d.Resource.Id == 2));
            Assert.True(query.Any(d => d.Resource.Id == 3));
            Assert.True(query.Any(d => d.Resource.Id == 6));
        }

        [Fact]
        public async Task CreateCustomer_ProsCompany_HasPermissionManageForDocument1()
        {
            var customer = await CreateCustomerAsync(ProsCompanyPrincipal);

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                1, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ProsCompany_HasPermissionManageForDocument2()
        {
            var customer = await CreateCustomerAsync(ProsCompanyPrincipal);

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                2, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ProsCompany_HasPermissionManageForDocument3()
        {
            var customer = await CreateCustomerAsync(ProsCompanyPrincipal);

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                3, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ProsCompany_HasNotPermissionsForDocument4()
        {
            var customer = await CreateCustomerAsync(ProsCompanyPrincipal);

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                4, ResourceStatus.Exists));
        }

        #endregion

        #region Normal User

        [Fact]
        public async Task CreateCustomer_NormalUser_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);
            var query = customer.DocumentAccessor.GetQuery();

            Assert.Equal(3, query.ToList().Count);
            Assert.True(query.Any(d => d.Resource.Id == 2));
            Assert.True(query.Any(d => d.Resource.Id == 3));
            Assert.True(query.Any(d => d.Resource.Id == 6));
        }

        [Fact]
        public async Task CreateCustomer_NormalUser_HasNotPermissionsForDocument1()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                1, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                1, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_NormalUser_HasAllPermissionsForDocument2()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                2, Permission.View, ResourceStatus.Exists));
            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                2, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_NormalUser_HasPermissionManageForDocument3()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(3, Permission.View,
                ResourceStatus.Exists));
            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(3, Permission.Manage,
                ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_NormalUser_HasNotPermissionsForDocument4()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                4, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                4, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_NormalUser_HasPermissionViewForDocument6()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                6, Permission.View, ResourceStatus.Archived));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                6, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_NormalUserAndNonExistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(-1, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(-1, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_NormalUserAndStatusExistsAndNonexistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(long.MaxValue, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.View, ResourceStatus.Exists));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_NormalUserAndStatusArchivedAndNonexistentDocument_Exception()
        {
            var customer = await CreateCustomerAsync(UserPrincipal);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(long.MaxValue, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.View, ResourceStatus.Archived));
            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.HasPermissionAsync(long.MaxValue, Permission.Manage, ResourceStatus.Archived));
        }

        #endregion

        #region User with administrate access to few companies

        [Fact]
        public async Task CreateCustomer_AdministrateAccessToFewCompanies_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(UserPrincipal2, new[] { 1, 2 });
            var query = customer.DocumentAccessor.GetQuery();

            Assert.Equal(5, query.ToList().Count);
            Assert.True(query.Any(d => d.Resource.Id == 1));
            Assert.True(query.Any(d => d.Resource.Id == 2));
            Assert.True(query.Any(d => d.Resource.Id == 3));
            Assert.True(query.Any(d => d.Resource.Id == 5));
            Assert.True(query.Any(d => d.Resource.Id == 6));
        }

        [Fact]
        public async Task CreateCustomer_AdministrateAccessToFewCompanies_HasPermissionManageForDocument1()
        {
            var customer = await CreateCustomerAsync(UserPrincipal2, new[] { 1, 2 });

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                1, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_AdministrateAccessToFewCompanies_HasPermissionManageForDocument2()
        {
            var customer = await CreateCustomerAsync(UserPrincipal2, new[] { 1, 2 });

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                2, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_AdministrateAccessToFewCompanies_HasPermissionManageForDocument3()
        {
            var customer = await CreateCustomerAsync(UserPrincipal2, new[] { 1, 2 });

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                3, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_AdministrateAccessToFewCompanies_HasNotPermissionsForDocument4()
        {
            var customer = await CreateCustomerAsync(UserPrincipal2, new[] { 1, 2 });

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                4, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_AdministrateAccessToFewCompanies_HasPermissionManageForDocument6()
        {
            var customer = await CreateCustomerAsync(UserPrincipal2, new[] { 1, 2 });

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                6, ResourceStatus.Archived));
        }

        #endregion

        #region User with claim "api.surveyrights"

        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);
            var query = customer.DocumentAccessor.GetQuery();

            Assert.Single(query.ToList());
            Assert.True(query.Any(d => d.Resource.Id == 4));
            Assert.Equal(Permission.Manage, query.FirstOrDefault()?.Permission);
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_HasNotPermissionsForDocument1()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                1, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_HasNotPermissionsForDocument2()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                2, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_HasNotPermissionsForDocument3()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                3, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_HasManagePermissionsForDocument4()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);

            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                4, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_HasNotPermissionsForDocument5()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                5, ResourceStatus.Exists));
        }


        [Fact]
        public async Task CreateCustomer_ClaimApiSurveyRights_HasNotPermissionsForDocument6()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiSurveyRights);

            Assert.Equal(Permission.None, await customer.DocumentAccessor.GetPermissionAsync(
                6, ResourceStatus.Archived));
        }

        #endregion

        #region User with claim "api.cdl.read"

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlRead_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            Assert.Equal(Context.Documents.Count(), customer.DocumentAccessor.GetQuery().ToList().Count);
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlRead_HasPermissionManageForDocument1()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            // There is explicit permission Manage
            Assert.Equal(Permission.Manage, await customer.DocumentAccessor.GetPermissionAsync(
                1, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlRead_HasHasPermissionViewForDocument2()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            Assert.Equal(Permission.View, await customer.DocumentAccessor.GetPermissionAsync(
                2, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlRead_HasPermissionViewForDocument3()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            Assert.Equal(Permission.View, await customer.DocumentAccessor.GetPermissionAsync(
                3, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlRead_HasHasPermissionViewForDocument4()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            Assert.Equal(Permission.View, await customer.DocumentAccessor.GetPermissionAsync(
                4, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlRead_HasPermissionViewForDocument5()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            Assert.Equal(Permission.View, await customer.DocumentAccessor.GetPermissionAsync(
                5, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlReadAndStatusIgnore_HasPermissionViewForDocument6()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            Assert.Equal(Permission.View, await customer.DocumentAccessor.GetPermissionAsync(
                6, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_ClaimApiCdlReadAndStatusExist_ExceptionForDocument6()
        {
            var customer = await CreateCustomerAsync(UserPrincipalWithClaimApiCdlRead);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                customer.DocumentAccessor.GetPermissionAsync(6, ResourceStatus.Exists));
        }

        #endregion

        #region Enduser

        [Fact]
        public async Task CreateCustomer_Enduser_DocumentQueryContainsAvailableDocuments()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);
            var query = customer.DocumentAccessor.GetQuery();

            Assert.Single(query.ToList());
            Assert.True(query.Any(d => d.Resource.Id == 5));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasNotPermissionsForDocument1()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                1, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                1, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasNotPermissionsForDocument2()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                2, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                2, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasNotPermissionsForDocument3()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                3, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                3, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasNotPermissionsForDocument4()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                4, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                4, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasPermissionViewForDocument5()
        {
            // Document 5 is the only document that has been published
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.True(await customer.DocumentAccessor.HasPermissionAsync(
                5, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                5, Permission.Manage, ResourceStatus.Exists));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasNotPermissionsForDocument6()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                6, Permission.View, ResourceStatus.Archived));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                6, Permission.Manage, ResourceStatus.Archived));
        }

        [Fact]
        public async Task CreateCustomer_Enduser_HasNotPermissionsForNonExistentDocument()
        {
            var customer = await CreateCustomerAsync(EnduserPrincipal);

            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                -1, Permission.View, ResourceStatus.Exists));
            Assert.False(await customer.DocumentAccessor.HasPermissionAsync(
                -1, Permission.Manage, ResourceStatus.Exists));
        }

        #endregion

        #region Helpers

        private ClaimsPrincipal AdminPrincipal => _fixture.AdminPrincipal;
        private ClaimsPrincipal ProsPrincipal => _fixture.ProsPrincipal;
        private ClaimsPrincipal CompanyAdminPrincipal => _fixture.CompanyAdminPrincipal;
        private ClaimsPrincipal ProsCompanyPrincipal => _fixture.ProsCompanyPrincipal;
        private ClaimsPrincipal UserPrincipal => _fixture.UserPrincipal;
        private ClaimsPrincipal UserPrincipal2 => _fixture.UserPrincipal2;
        private ClaimsPrincipal EnduserPrincipal => _fixture.EnduserPrincipal;
        private ClaimsPrincipal UserPrincipalWithClaimApiSurveyRights => _fixture.UserPrincipalWithClaimApiSurveyRights;
        private ClaimsPrincipal UserPrincipalWithClaimApiCdlRead => _fixture.UserPrincipalWithClaimApiCdlRead;

        private CdlDbContext Context => _fixture.Context;

        private async Task<ICustomer> CreateCustomerAsync(ClaimsPrincipal principal, int[] adminAccessCompanies = null)
        {
            return await new Factory(
                Context, principal, new AccountLoaderStub(adminAccessCompanies)).CreateCustomerAsync();
        }

        #endregion
    }
}
