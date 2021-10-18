using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class UserPermissionTests : TestBase
    {
        public UserPermissionTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        #region GetUserPermissions

        [Fact]
        public async Task GetUserPermissions_FirstPage_ValidPermissionsAndLinks()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            var page = await GetUserPermissionsAsync(document.Id, skip: 0, top: 1, orderBy: "Name desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(ProsUser.Id, item.Id);
            Assert.Equal(ProsUser.Name, item.Name);
            Assert.Equal(ProsUser.FullName, item.FullName);
            Assert.Equal(ProsUser.CompanyId, item.CompanyId);
            Assert.Equal(ProsUser.Company.Name, item.CompanyName);
            Assert.Equal(Permission.Manage, item.Permission);
            var expected = $"/api/cdl/documents/{document.Id}/userpermissions?$skip=1&$top=1&$orderby=Name desc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
            Assert.Null(page.Links.PreviousPage);
        }

        [Fact]
        public async Task GetUserPermissions_SecondPage_ValidPermissionsAndLinks()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            var page = await GetUserPermissionsAsync(document.Id, skip: 1, top: 1, orderBy: "Name desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(NormalUser.Id, item.Id);
            Assert.Equal(NormalUser.Name, item.Name);
            Assert.Equal(NormalUser.FullName, item.FullName);
            Assert.Equal(NormalUser.CompanyId, item.CompanyId);
            Assert.Equal(NormalUser.Company.Name, item.CompanyName);
            Assert.Equal(Permission.View, item.Permission);
            Assert.Null(page.Links.NextPage);
            var expected = $"/api/cdl/documents/{document.Id}/userpermissions?$skip=0&$top=1&$orderby=Name desc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.PreviousPage));
        }

        [Fact]
        public async Task GetUserPermissions_SortByIdAsc_CorrectOrder()
        {
            await UseCompanyAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = ProsUser.Id, Permission = Permission.Manage },
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "Id asc");

            Assert.Equal(3, page.TotalCount);
            Assert.Equal(3, page.Items.Count); // Includes permission of CompanyAdmin
            page.Items.Remove(page.Items.FirstOrDefault(p => p.Id == CompanyAdmin.Id));
            if (NormalUser.Id > ProsUser.Id)
            {
                Assert.Equal(ProsUser.Id, page.Items[0].Id);
                Assert.Equal(NormalUser.Id, page.Items[1].Id);
            }
            else
            {
                Assert.Equal(NormalUser.Id, page.Items[0].Id);
                Assert.Equal(ProsUser.Id, page.Items[1].Id);
            }
        }

        [Fact]
        public async Task GetUserPermissions_SortByIdDesc_CorrectOrder()
        {
            await UseCompanyAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = ProsUser.Id, Permission = Permission.Manage },
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "Id desc");

            Assert.Equal(3, page.TotalCount);
            Assert.Equal(3, page.Items.Count); // Includes permission of CompanyAdmin
            page.Items.Remove(page.Items.FirstOrDefault(p => p.Id == CompanyAdmin.Id));
            if (NormalUser.Id > ProsUser.Id)
            {
                Assert.Equal(NormalUser.Id, page.Items[0].Id);
                Assert.Equal(ProsUser.Id, page.Items[1].Id);
            }
            else
            {
                Assert.Equal(ProsUser.Id, page.Items[0].Id);
                Assert.Equal(NormalUser.Id, page.Items[1].Id);
            }
        }

        [Fact]
        public async Task GetUserPermissions_SortByNameAsc_CorrectOrder()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = ProsUser.Id, Permission = Permission.Manage }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "Name asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count); // Includes permission of NormalUser
            Assert.Equal(NormalUser.Name, page.Items[0].Name);
            Assert.Equal(ProsUser.Name, page.Items[1].Name);
        }

        [Fact]
        public async Task GetUserPermissions_SortByNameDesc_CorrectOrder()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = ProsUser.Id, Permission = Permission.Manage }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "Name desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count); // Includes permission of NormalUser
            Assert.Equal(ProsUser.Name, page.Items[0].Name);
            Assert.Equal(NormalUser.Name, page.Items[1].Name);
        }

        [Fact]
        public async Task GetUserPermissions_SortByFullNameAsc_CorrectOrder()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = ProsUser.Id, Permission = Permission.Manage }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "FullName asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count); // Includes permission of NormalUser
            Assert.Equal(NormalUser.FullName, page.Items[0].FullName);
            Assert.Equal(ProsUser.FullName, page.Items[1].FullName);
        }

        [Fact]
        public async Task GetUserPermissions_SortByFullNameDesc_CorrectOrder()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = ProsUser.Id, Permission = Permission.Manage }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "FullName desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count); // Includes permission of NormalUser
            Assert.Equal(ProsUser.Id, page.Items[0].Id);
            Assert.Equal(NormalUser.Id, page.Items[1].Id);
        }

        [Fact]
        public async Task GetUserPermissions_SortByCompanyIdAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "CompanyId asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Admin.CompanyId, page.Items[0].CompanyId);
            Assert.Equal(NormalUser.CompanyId, page.Items[1].CompanyId);
        }

        [Fact]
        public async Task GetUserPermissions_SortByCompanyIdDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "CompanyId desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(NormalUser.CompanyId, page.Items[0].CompanyId);
            Assert.Equal(Admin.CompanyId, page.Items[1].CompanyId);
        }

        [Fact]
        public async Task GetUserPermissions_SortByCompanyNameAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "CompanyName asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal("Confirmit", page.Items[0].CompanyName);
            Assert.Equal("TestCompany", page.Items[1].CompanyName);
        }

        [Fact]
        public async Task GetUserPermissions_SortByCompanyNameDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "CompanyName desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal("TestCompany", page.Items[0].CompanyName);
            Assert.Equal("Confirmit", page.Items[1].CompanyName);
        }

        [Fact]
        public async Task GetUserPermissions_SortByPermissionAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.Manage },
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "Permission asc");

            Assert.Equal(3, page.TotalCount);
            Assert.Equal(3, page.Items.Count);
            Assert.Equal(Permission.View, page.Items[0].Permission);
            Assert.Equal(Permission.Manage, page.Items[1].Permission);
            Assert.Equal(Permission.Manage, page.Items[2].Permission);
        }

        [Fact]
        public async Task GetUserPermissions_SortByPermissionDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.Manage },
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, orderBy: "Permission desc");

            Assert.Equal(3, page.TotalCount);
            Assert.Equal(3, page.Items.Count);
            Assert.Equal(Permission.Manage, page.Items[0].Permission);
            Assert.Equal(Permission.Manage, page.Items[1].Permission);
            Assert.Equal(Permission.View, page.Items[2].Permission);
        }

        [Fact]
        public async Task GetUserPermissions_SearchById_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(NormalUser.Id, item.Id);
        }

        [Fact]
        public async Task GetUserPermissions_SearchByName_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"Name eq '{NormalUser.Name}'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(NormalUser.Name, item.Name);
        }

        [Fact]
        public async Task GetUserPermissions_SearchByFullName_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"FullName eq '{NormalUser.FullName}'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(NormalUser.FullName, item.FullName);
        }

        [Fact]
        public async Task GetUserPermissions_SearchByCompanyId_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"CompanyId eq {NormalUser.CompanyId}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(NormalUser.CompanyId, item.CompanyId);
        }

        [Fact]
        public async Task GetUserPermissions_SearchByCompanyName_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"CompanyName eq '{NormalUser.Company.Name}'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(NormalUser.Company.Name, item.CompanyName);
        }

        [Fact]
        public async Task GetUserPermissions_SearchByPermission_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            var page = await GetUserPermissionsAsync(document.Id, filter: "Permission eq 'View'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal("View", item.Permission.ToString());
        }

        [Fact]
        public async Task GetUserPermissions_SearchByWrongPermission_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            await GetUserPermissionsAsync(document.Id, filter: "Permission eq 'WRONG'",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedErrorMessage: "The string 'WRONG' is not a valid enumeration type constant.");
        }

        [Fact]
        public async Task GetUserPermissions_SearchByWrongName_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            await GetUserPermissionsAsync(document.Id, filter: "WRONG eq 'aaa'",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedErrorMessage: "Could not find a property named 'WRONG' on type 'Confirmit.Cdl.Api.ViewModel.UserPermissionFullDto'.");
        }

        [Fact]
        public async Task GetUserPermissions_NormalUserWithoutPermissionOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await GetUserPermissionsAsync(document.Id, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetUserPermissions_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();

            // Due to principle of least privilege user with view permission should not have access to permissions
            // because he cannot set them
            await GetUserPermissionsAsync(document.Id, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetUserPermissions_Enduser_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseEnduserAsync();
            await GetUserPermissionsAsync(document.Id, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDocumentUserPermission_NormalUser_PermissionHasUserAndCompanyName()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            // Service will update user and company names here
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            var permission = (await GetUserPermissionsAsync(document.Id)).Items.FirstOrDefault(i => i.Id == NormalUser.Id);

            Assert.NotNull(permission);
            Assert.Equal(NormalUser.Name, permission.Name);
            Assert.Equal(NormalUser.FullName, permission.FullName);
            Assert.Equal(NormalUser.CompanyId, permission.CompanyId);
            Assert.Equal(NormalUser.Company.Name, permission.CompanyName);
        }

        #endregion

        #region GetUserPermissionsCompanies

        [Fact]
        public async Task GetUserPermissionsCompanies_ConfirmitAndTestCompanies_ResultContainsBothOnes()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync(); // There is permission for Admin (Confirmit)
            await PatchUserPermissionsAsync(document.Id,
                new[]
                {
                    new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
                });

            var companies = (await GetUserPermissionsCompaniesAsync(document.Id)).Items;
            var ids = companies.Select(l => l.Id).Distinct();

            Assert.Equal(ids.Count(), companies.Count);
            Assert.NotNull(companies.SingleOrDefault(c => c.Name == "Confirmit"));
            Assert.NotNull(companies.SingleOrDefault(c => c.Name == "TestCompany"));
        }

        [Fact]
        public async Task GetUserPermissionsCompanies_SearchByName_ValidEnduserListsAndLinks()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync(); // There is permission for Admin (Confirmit)
            await PatchUserPermissionsAsync(document.Id,
                new[]
                {
                    new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
                });

            var page = await GetUserPermissionsCompaniesAsync(document.Id, skip: 0, top: 1, orderBy: "Name asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(Admin.Company.Id, item.Id);
            Assert.Equal(Admin.Company.Name, item.Name);
            var expected = $"/api/cdl/documents/{document.Id}/userpermissions/companies?$skip=1&$top=1&$orderby=Name asc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
            Assert.Null(page.Links.PreviousPage);
        }

        [Fact]
        public async Task GetUserPermissionsCompanies_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();

            // Due to principle of least privilege user with view permission should not have access to used companies
            // because he cannot set user permissions
            await GetUserPermissionsCompaniesAsync(document.Id, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        #endregion

        #region PatchUserPermissions

        [Fact]
        public async Task PatchUserPermissions_EmptyList_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new UserPermissionDto[] { }, HttpStatusCode.BadRequest,
                "Empty permission list");
        }

        [Fact]
        public async Task PatchUserPermissions_DuplicateEntry_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage },
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            }, HttpStatusCode.BadRequest, "Duplicate entry in permission list");
        }

        [Fact]
        public async Task PatchUserPermissions_NonexistentUser_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = int.MaxValue, Permission = Permission.View } },
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PatchUserPermissions_WrongPermission_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new PermissionStub { Id = NormalUser.Id, Permission = "WrongCode" } },
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PatchUserPermissions_NeitherIdNorUserKeyIsSpecified_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Permission = Permission.View } },
                HttpStatusCode.BadRequest, "Neither Id nor userKey is specified");
        }

        [Fact]
        public async Task PatchUserPermissions_Twice_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });
        }

        [Fact]
        public async Task PatchUserPermissions_Twice_LastPermissionIsInEffect()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Permission.Manage, page.Items[0].Permission);
        }

        [Fact]
        public async Task PatchUserPermissions_PermissionNone_UserDeleted()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(NormalUser.Id, page.Items[0].Id);
            Assert.Equal(Permission.View, page.Items[0].Permission);

            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.None } });

            page = await GetUserPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.Id}");

            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task PatchUserPermissions_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchUserPermissions_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchUserPermissions_NormalUserWithPermissionManageOnDocument_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.View } });
        }

        [Fact]
        public async Task PatchUserPermissions_ForUserFromOtherCompany_Ok()
        {
            await UseAdminAsync();
            // Create document
            var document = await PostDocumentAsync();
            // Grant Manage permission to NormalUser
            await PatchUserPermissionsAsync(document.Id, new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });
            // Give user from other company permission View
            await PatchUserPermissionsAsync(document.Id, new[] { new UserPermissionDto { Id = NormalUser2.Id, Permission = Permission.View } });

            // Try to change permission for the user from other company under NormalUser account
            await UseNormalUserAsync();
            await PatchUserPermissionsAsync(document.Id, new[] { new UserPermissionDto { Id = NormalUser2.Id, Permission = Permission.Manage } });

            // Successfully changed
            var permissions = (await GetUserPermissionsAsync(document.Id)).Items;
            Assert.Contains(permissions, p => p.Id == NormalUser2.Id && p.Permission == Permission.Manage);
        }

        #endregion

        #region DeleteIndividualUserPermissions

        [Fact]
        public async Task DeleteIndividualUserPermissions_NonexistentCompany_NotFound()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();

            await DeleteIndividualUserPermissionsAsync(document.Id, 0,
                HttpStatusCode.NotFound, "No individual permissions for company 0");
        }

        [Fact]
        public async Task DeleteIndividualUserPermissions_Deleted()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await DeleteIndividualUserPermissionsAsync(document.Id, NormalUser.CompanyId);

            Assert.Empty((await GetUserPermissionsAsync(document.Id, filter: $"CompanyId eq {NormalUser.CompanyId}"))
                .Items);
        }

        [Fact]
        public async Task DeleteIndividualUserPermissions_Twice_NotFound()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await DeleteIndividualUserPermissionsAsync(document.Id, NormalUser.CompanyId);
            await DeleteIndividualUserPermissionsAsync(document.Id, NormalUser.CompanyId,
                HttpStatusCode.NotFound, $"No individual permissions for company {NormalUser.CompanyId}");
        }

        #endregion

        #region DeleteUserPermission

        [Fact]
        public async Task DeleteUserPermission_DeleteOwnPermission_NoPermissions()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();

            // Delete own permission
            await DeleteUserPermissionAsync(document.Id, NormalUser.Id);

            await UseAdminAsync();
            var page = await GetUserPermissionsAsync(document.Id);

            Assert.Equal(0, page.TotalCount);
            Assert.True(page.Items.Count == 0, "List of permissions should be empty");
        }

        [Fact]
        public async Task DeleteUserPermission_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await DeleteUserPermissionAsync(document.Id, NormalUser.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteUserPermission_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            await DeleteUserPermissionAsync(document.Id, NormalUser.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteUserPermission_NormalUserWithPermissionManageOnDocument_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            await DeleteUserPermissionAsync(document.Id, NormalUser.Id);
        }

        #endregion
    }
}
