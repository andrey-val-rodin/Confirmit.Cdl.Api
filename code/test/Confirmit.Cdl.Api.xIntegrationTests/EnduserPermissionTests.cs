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
    public class EnduserPermissionTests : TestBase
    {
        public EnduserPermissionTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        #region GetEnduserPermissions

        [Fact]
        public async Task GetEnduserPermissions_FirstPage_ValidPermissionsAndLinks()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, skip: 0, top: 1, orderBy: "Id");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(Enduser.Id, item.Id);
            Assert.Equal(Enduser.Name, item.Name);
            Assert.Equal(Enduser.FullName, item.FullName);
            Assert.Equal(Enduser.List.Id, item.EnduserListId);
            Assert.Equal(Enduser.List.Name, item.EnduserListName);
            Assert.Equal(Permission.View, item.Permission);
            var expected = $"/api/cdl/documents/{document.Id}/enduserpermissions?$skip=1&$top=1&$orderby=Id";
            AssertValidLinks(expected, page.Links.NextPage);
            Assert.Null(page.Links.PreviousPage);
        }

        [Fact]
        public async Task GetEnduserPermissions_SecondPage_ValidPermissionsAndLinks()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, skip: 1, top: 1);

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(Enduser2.Id, item.Id);
            Assert.Equal(Enduser2.Id, item.Id);
            Assert.Equal(Enduser2.Name, item.Name);
            Assert.Equal(Enduser2.FullName, item.FullName);
            Assert.Equal(Enduser2.List.Id, item.EnduserListId);
            Assert.Equal(Enduser2.List.Name, item.EnduserListName);
            Assert.Equal(Permission.View, item.Permission);
            Assert.Null(page.Links.NextPage);
            var expected = $"/api/cdl/documents/{document.Id}/enduserpermissions?$skip=0&$top=1";
            AssertValidLinks(expected, page.Links.PreviousPage);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByIdAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "Id asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
            Assert.Equal(Enduser2.Id, page.Items[1].Id);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByIdDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "Id desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser2.Id, page.Items[0].Id);
            Assert.Equal(Enduser.Id, page.Items[1].Id);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByNameAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "Name asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser.Name, page.Items[0].Name);
            Assert.Equal(Enduser2.Name, page.Items[1].Name);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByNameDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "Name desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser2.Name, page.Items[0].Name);
            Assert.Equal(Enduser.Name, page.Items[1].Name);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByFullNameAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "FullName asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser.FullName, page.Items[0].FullName);
            Assert.Equal(Enduser2.FullName, page.Items[1].FullName);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByFullNameDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "FullName desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser2.FullName, page.Items[0].FullName);
            Assert.Equal(Enduser.FullName, page.Items[1].FullName);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByEnduserListIdAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "EnduserListId asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser.ListId, page.Items[0].EnduserListId);
            Assert.Equal(Enduser3.ListId, page.Items[1].EnduserListId);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByEnduserListIdDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "EnduserListId desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser3.ListId, page.Items[0].EnduserListId);
            Assert.Equal(Enduser.ListId, page.Items[1].EnduserListId);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByEnduserListNameAsc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "EnduserListName asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser.List.Name, page.Items[0].EnduserListName);
            Assert.Equal(Enduser3.List.Name, page.Items[1].EnduserListName);
        }

        [Fact]
        public async Task GetEnduserPermissions_SortByEnduserListNameDesc_CorrectOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, orderBy: "EnduserListName desc");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(Enduser3.List.Name, page.Items[0].EnduserListName);
            Assert.Equal(Enduser.List.Name, page.Items[1].EnduserListName);
        }

        [Fact]
        public async Task GetEnduserPermissions_SearchById_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"Id eq {Enduser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
        }

        [Fact]
        public async Task GetEnduserPermissions_SearchByName_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"Name eq '{Enduser2.Name}'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser2.Name, page.Items[0].Name);
        }

        [Fact]
        public async Task GetEnduserPermissions_SearchByFullName_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"FullName eq '{Enduser.FullName}'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.FullName, page.Items[0].FullName);
        }

        [Fact]
        public async Task GetEnduserPermissions_SearchByEnduserListId_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"EnduserListId eq {Enduser3.ListId}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser3.ListId, page.Items[0].EnduserListId);
        }

        [Fact]
        public async Task GetEnduserPermissions_SearchByEnduserListName_ReturnsAvailablePermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id,
                filter: $"EnduserListName eq '{Enduser.List.Name}'");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.List.Name, page.Items[0].EnduserListName);
        }

        [Fact]
        public async Task GetEnduserPermissions_SearchByWrongName_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
            });

            await GetEnduserPermissionsAsync(document.Id, filter: "WRONG eq 'aaa'",
                expectedStatusCode: HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetEnduserPermissions_NormalUserWithoutPermissionOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await GetEnduserPermissionsAsync(document.Id, null, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetEnduserPermissions_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });

            await UseNormalUserAsync();

            // Due to principle of least privilege user with view permission should not have access to permissions
            // because he cannot set them
            await GetEnduserPermissionsAsync(document.Id, null, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetEnduserPermissions_Enduser_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseEnduserAsync();
            await GetEnduserPermissionsAsync(document.Id, null, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetEnduserPermissions_Enduser_PermissionHasEnduserAndEnduserListName()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            // Service will update enduser and enduser list names here
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            });

            var permission =
                (await GetEnduserPermissionsAsync(document.Id)).Items.FirstOrDefault(i => i.Id == Enduser.Id);

            Assert.NotNull(permission);
            Assert.Equal(Enduser.Name, permission.Name);
            Assert.Equal(Enduser.FullName, permission.FullName);
            Assert.Equal(Enduser.ListId, permission.EnduserListId);
            Assert.Equal(Enduser.List.Name, permission.EnduserListName);
        }

        #endregion

        #region GetEnduserPermissionsEnduserLists

        [Fact]
        public async Task GetEnduserPermissionsEnduserLists_EnduserListAndEnduserList2_ResultContainsBothOnes()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[]
                {
                    new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                    new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
                });

            var enduserLists = (await GetEnduserPermissionsEnduserListsAsync(document.Id)).Items;
            var ids = enduserLists.Select(l => l.Id).Distinct();

            Assert.Equal(ids.Count(), enduserLists.Count);
            Assert.NotNull(enduserLists.SingleOrDefault(c => c.Id == EnduserList.Id));
            Assert.NotNull(enduserLists.SingleOrDefault(c => c.Id == EnduserList2.Id));
        }

        [Fact]
        public async Task GetEnduserPermissionsEnduserLists_SearchByName_ValidEnduserListsAndLinks()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[]
                {
                    new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                    new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
                });

            var page = await GetEnduserPermissionsEnduserListsAsync(document.Id, skip: 0, top: 1, orderBy: "Name asc");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            var item = page.Items[0];
            Assert.Equal(Enduser.List.Id, item.Id);
            Assert.Equal(Enduser.List.Name, item.Name);
            var expected =
                $"/api/cdl/documents/{document.Id}/enduserpermissions/enduserlists?$skip=1&$top=1&$orderby=Name asc";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
            Assert.Null(page.Links.PreviousPage);
        }

        #endregion

        #region GetEnduserPermissionsForEnduserList

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_NoInactiveEnduser()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var permissions = (await GetEnduserPermissionsForEnduserListAsync(document.Id, EnduserList.Id)).Items;

            Assert.Equal(2, permissions.Count);
            Assert.DoesNotContain(permissions, p => p.Id == InactiveEnduser.Id);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_SortByIdAsc_ValidOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var permissions =
                (await GetEnduserPermissionsForEnduserListAsync(document.Id, EnduserList.Id, orderBy: "Id asc")).Items;

            Assert.Equal(Enduser.Id, permissions[0].Id);
            Assert.Equal(Enduser2.Id, permissions[1].Id);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_SortByIdDesc_ValidOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var permissions = (await GetEnduserPermissionsForEnduserListAsync(
                document.Id, EnduserList.Id, orderBy: "Id desc")).Items;

            Assert.Equal(Enduser2.Id, permissions[0].Id);
            Assert.Equal(Enduser.Id, permissions[1].Id);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_SortByFullNameAsc_ValidOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var permissions = (await GetEnduserPermissionsForEnduserListAsync(
                document.Id, EnduserList.Id, orderBy: "FullName asc")).Items;

            Assert.Equal(Enduser.FullName, permissions[0].FullName);
            Assert.Equal(Enduser2.FullName, permissions[1].FullName);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_SortByFullNameDesc_ValidOrder()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var permissions = (await GetEnduserPermissionsForEnduserListAsync(
                document.Id, EnduserList.Id, orderBy: "FullName desc")).Items;

            Assert.Equal(Enduser2.FullName, permissions[0].FullName);
            Assert.Equal(Enduser.FullName, permissions[1].FullName);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_SearchById_ValidResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var page = await GetEnduserPermissionsForEnduserListAsync(
                document.Id, EnduserList.Id, filter: $"Id eq {Enduser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_FirstPage_ValidPageAndLinks()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            var page = await GetEnduserPermissionsForEnduserListAsync(
                document.Id, EnduserList.Id, skip: 0, top: 1, orderBy: "Id asc", filter: "startswith(Name, 'Enduser')");

            Assert.Equal(2, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
            var expected =
                $"/api/cdl/documents/{document.Id}/enduserpermissions/enduserlists/{EnduserList.Id}?$skip=1&$top=1&$orderby=Id asc&$filter=startswith(Name, 'Enduser')";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
            Assert.Null(page.Links.PreviousPage);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_Admin_ValidListWithPermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            var page = await GetEnduserPermissionsForEnduserListAsync(document.Id, Enduser.ListId);
            AssertValidListWithPermissions(page);
        }

        [Fact]
        public async Task
            GetEnduserPermissionsForEnduserList_ThereIsPermissionForWholeList_WholeListPermissionDoesNotAffectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = Enduser.ListId, Permission = Permission.View });

            var page = await GetEnduserPermissionsForEnduserListAsync(document.Id, Enduser.ListId);
            AssertValidListWithPermissions(page);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_NormalUserWithoutAccessToEnduserList_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            // NotFound expected because normal user has not permission to read enduser list
            await GetEnduserPermissionsForEnduserListAsync(document.Id, EnduserList.Id,
                expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetEnduserPermissionsForEnduserList_Enduser_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await UseEnduserAsync();
            // NotFound expected because enduser has not permission to read enduser list
            await GetEnduserPermissionsForEnduserListAsync(document.Id, EnduserList.Id,
                expectedStatusCode: HttpStatusCode.Forbidden);
        }

        #endregion

        #region PatchEnduserPermissions

        [Fact]
        public async Task PatchEnduserPermissions_EmptyList_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new PermissionDto[] { }, HttpStatusCode.BadRequest,
                "Empty permission list.");
        }

        [Fact]
        public async Task PatchEnduserPermissions_DuplicateEntry_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            }, HttpStatusCode.BadRequest, "Duplicate entry in permission list.");
        }

        [Fact]
        public async Task PatchEnduserPermissions_NonexistentEnduser_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = 0, Permission = Permission.View } },
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PatchEnduserPermissions_PermissionManage_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.Manage } },
                HttpStatusCode.BadRequest, "Only permissions None or View can be specified for enduser");
        }

        [Fact]
        public async Task PatchEnduserPermissions_WrongPermission_BadRequest()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionStub { Id = Enduser.Id, Permission = "WrongCode" } },
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PatchEnduserPermissions_Twice_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
        }

        [Fact]
        public async Task PatchEnduserPermissions_Twice_LastPermissionIsInEffect()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.None } });

            var page = await GetUserPermissionsAsync(document.Id, filter: $"Id eq {NormalUser.Id}");

            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task PatchEnduserPermissions_PermissionNone_EnduserDeleted()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"Id eq {Enduser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
            Assert.Equal(Permission.View, page.Items[0].Permission);

            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.None } });
            page = await GetEnduserPermissionsAsync(document.Id, filter: $"Id eq {Enduser.Id}");

            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task PatchEnduserPermissions_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchEnduserPermissions_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchEnduserPermissions_ProsUserHasAccessToEnduserList_Ok()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
        }

        [Fact]
        public async Task PatchEnduserPermissions_EnduserHasAccessToPublishedRevision()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PostRevisionAsync(document.Id);
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await UseEnduserAsync();
            await GetPublishedRevisionAsync(document.Id);
        }

        [Fact]
        public async Task PatchEnduserPermissions_InactiveEnduser_Ignored()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PostRevisionAsync(document.Id);
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = InactiveEnduser.Id, Permission = Permission.View } });

            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"Id eq {InactiveEnduser.Id}");

            Assert.Equal(0, page.TotalCount);
        }

        [Fact]
        public async Task PatchEnduserPermissions_EnduserAndInactiveEnduser_InactiveEnduserIgnored()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PostRevisionAsync(document.Id);
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = InactiveEnduser.Id, Permission = Permission.View }
            });

            var page = await GetEnduserPermissionsAsync(document.Id);

            Assert.Equal(1, page.TotalCount);
            Assert.Equal(1, page.ItemCount);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
        }

        [Fact]
        public async Task PatchEnduserPermissions_ThereIsEnduserListPermission_EnduserListPermissionStillThere()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PostRevisionAsync(document.Id);
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            Assert.Single((await GetEnduserListPermissionsAsync(document.Id)).Items);
        }

        #endregion

        #region DeleteIndividualEnduserListPermissions

        [Fact]
        public async Task DeleteIndividualEnduserListPermissions_NonexistentEnduserList_NotFound()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();

            await DeleteIndividualEnduserListPermissionsAsync(document.Id, 0,
                HttpStatusCode.NotFound, "No individual permissions for enduser list 0");
        }

        [Fact]
        public async Task DeleteIndividualEnduserListPermissions_Deleted()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await DeleteIndividualEnduserListPermissionsAsync(document.Id, Enduser.ListId);

            Assert.Empty((await GetEnduserPermissionsAsync(document.Id, filter: $"EnduserListId eq {Enduser.ListId}"))
                .Items);
        }

        [Fact]
        public async Task DeleteIndividualEnduserListPermissions_twice_NotFound()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await DeleteIndividualEnduserListPermissionsAsync(document.Id, Enduser.ListId);
            await DeleteIndividualEnduserListPermissionsAsync(document.Id, Enduser.ListId,
                HttpStatusCode.NotFound, $"No individual permissions for enduser list {Enduser.ListId}");
        }

        #endregion

        #region DeleteEnduserPermission

        [Fact]
        public async Task DeleteEnduserPermission_NoPermissions()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await DeleteEnduserPermissionAsync(document.Id, Enduser.Id);

            var page = await GetEnduserPermissionsAsync(document.Id);

            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task DeleteEnduserPermission_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            });

            await UseNormalUserAsync();
            await DeleteEnduserPermissionAsync(document.Id, Enduser.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteEnduserPermission_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View }
            });
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            });

            await UseNormalUserAsync();
            await DeleteEnduserPermissionAsync(document.Id, Enduser.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteEnduserPermission_NormalUserWithPermissionManageOnDocument_Ok()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id, new[]
            {
                new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage }
            });
            await PatchEnduserPermissionsAsync(document.Id, new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            });

            await UseNormalUserAsync();
            await DeleteEnduserPermissionAsync(document.Id, Enduser.Id);
        }

        #endregion

        #region Helpers

        private void AssertValidListWithPermissions(PageDto<EnduserPermissionFullDto> page)
        {
            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            var item = page.Items[0];
            Assert.Equal(Enduser.Id, item.Id);
            Assert.Equal(Enduser.Name, item.Name);
            Assert.Equal(Enduser.FullName, item.FullName);
            Assert.Equal(Enduser.Email, item.Email);
            Assert.Equal(Enduser.ListId, item.EnduserListId);
            Assert.Equal(Enduser.CompanyId, item.EnduserCompanyId);
            Assert.Equal(Enduser.List.Name, item.EnduserListName);
            Assert.Equal(Enduser.Company.Name, item.EnduserCompanyName);
            Assert.Equal(Permission.View, item.Permission);
            var item2 = page.Items[1];
            Assert.Equal(Enduser2.Id, item2.Id);
            Assert.Equal(Enduser2.Name, item2.Name);
            Assert.Equal(Enduser2.FullName, item2.FullName);
            Assert.Equal(Enduser2.Email, item2.Email);
            Assert.Equal(Enduser2.ListId, item2.EnduserListId);
            Assert.Equal(Enduser2.CompanyId, item2.EnduserCompanyId);
            Assert.Equal(Enduser2.List.Name, item2.EnduserListName);
            Assert.Equal(Enduser2.Company.Name, item2.EnduserCompanyName);
            Assert.Equal(Permission.None, item2.Permission);
        }

        #endregion
    }
}
