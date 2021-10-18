using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class PermissionTests : TestBase
    {
        public PermissionTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task GetPermission_Admin_PermissionManage()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var permission = await GetPermissionAsync(document.Id);

            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_AdminSpecifiesInvalidId_NotFound()
        {
            await UseAdminAsync();
            await GetPermissionAsync(0, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPermission_NormalUserWithGrantedPermissionView_PermissionView()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);

            Assert.Equal(Permission.View, permission);
        }

        [Fact]
        public async Task GetPermission_NormalUserWithGrantedPermissionView_PermissionIsEqualToPermissionInDocumentList()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var document2 = documents.SingleOrDefault(d => d.Id == document.Id);

            Assert.NotNull(document2);
            Assert.Equal(permission, document2.Permission);
        }

        [Fact]
        public async Task GetPermission_NormalUserWithGrantedPermissionManage_PermissionIsEqualToPermissionInDocumentList()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var document2 = documents.SingleOrDefault(d => d.Id == document.Id);

            Assert.NotNull(document2);
            Assert.Equal(permission, document2.Permission);
        }

        [Fact]
        public async Task GetPermission_NormalUserAndDifferentCompanyAndIndividualPermissions_MaximumPermission()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = NormalUser.CompanyId, Permission = Permission.Manage });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);

            // Company permission is Manage, therefore NormalUser has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_NormalUserAndDifferentCompanyAndIndividualPermissions2_MaximumPermission()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = NormalUser.CompanyId, Permission = Permission.View });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);

            // Permission of NormalUser is Manage, therefore NormalUser has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_NormalUserAndDifferentCompanyAndIndividualPermissions_PermissionIsEqualToPermissionInDocumentList()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = NormalUser.CompanyId, Permission = Permission.Manage });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var document2 = documents.SingleOrDefault(d => d.Id == document.Id);

            Assert.NotNull(document2);
            // Company permission is Manage, therefore NormalUser has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_NormalUserAndDifferentCompanyAndIndividualPermissions2_PermissionIsEqualToPermissionInDocumentList()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = NormalUser.CompanyId, Permission = Permission.View });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            var permission = await GetPermissionAsync(document.Id);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var document2 = documents.SingleOrDefault(d => d.Id == document.Id);

            Assert.NotNull(document2);
            // Permission of NormalUser is Manage, therefore NormalUser has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }


        [Fact]
        public async Task GetPermission_CompanyAdminAndDifferentCompanyAndIndividualPermissions_MaximumPermission()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = CompanyAdmin.CompanyId, Permission = Permission.Manage });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.View } });

            await UseCompanyAdminAsync();
            // The service uses explicit permissions only because CompanyAdmin has administrative access to another company
            var permission = await GetPermissionAsync(document.Id);

            // Company permission is Manage, therefore CompanyAdmin has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }
        [Fact]
        public async Task GetPermission_CompanyAdminAndDifferentCompanyAndIndividualPermissions2_MaximumPermission()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = CompanyAdmin.CompanyId, Permission = Permission.View });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.Manage } });

            await UseCompanyAdminAsync();
            var permission = await GetPermissionAsync(document.Id);

            // Permission of CompanyAdmin is Manage, therefore CompanyAdmin has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_CompanyAdminAndDifferentCompanyAndIndividualPermissions_PermissionIsEqualToPermissionInDocumentList()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = CompanyAdmin.CompanyId, Permission = Permission.Manage });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.View } });

            await UseCompanyAdminAsync();
            var permission = await GetPermissionAsync(document.Id);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var document2 = documents.SingleOrDefault(d => d.Id == document.Id);

            Assert.NotNull(document2);
            // Company permission is Manage, therefore CompanyAdmin has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_CompanyAdminAndDifferentCompanyAndIndividualPermissions2_PermissionIsEqualToPermissionInDocumentList()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PutCompanyPermissionAsync(document.Id, new PermissionDto
                { Id = CompanyAdmin.CompanyId, Permission = Permission.View });
            await PatchUserPermissionsAsync(document.Id, new[]
                { new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.Manage } });

            await UseCompanyAdminAsync();
            var permission = await GetPermissionAsync(document.Id);
            var documents = (await GetDocumentsAsync(filter: $"Name eq '{Prefix + name}'")).Items;
            var document2 = documents.SingleOrDefault(d => d.Id == document.Id);

            Assert.NotNull(document2);
            // Permission of CompanyAdmin is Manage, therefore CompanyAdmin has Manage permission
            Assert.Equal(Permission.Manage, permission);
        }

        [Fact]
        public async Task GetPermission_EnduserWithoutPermission_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseEnduserAsync();
            await GetPermissionAsync(document.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPermission_EnduserWithPermissionButDocumentHasNotPublishedRevision_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new []
                { new PermissionDto { Id = Enduser.Id, Permission = Permission.View} });

            await UseEnduserAsync();
            await GetPermissionAsync(document.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPermission_EnduserWithGrantedPermissionView_PermissionView()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new []
                { new PermissionDto { Id = Enduser.Id, Permission = Permission.View} });
            await PostRevisionAsync(document.Id);

            await UseEnduserAsync();
            var permission = await GetPermissionAsync(document.Id);

            Assert.Equal(Permission.View, permission);
        }
    }
}
