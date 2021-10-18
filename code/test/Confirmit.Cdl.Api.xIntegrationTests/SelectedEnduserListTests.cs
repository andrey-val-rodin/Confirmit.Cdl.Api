using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class SelectedEnduserListTests : TestBase
    {
        public SelectedEnduserListTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task PatchDocumentEnduserPermissions_ThereAreNewSelectedEnduserLists()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            // Two enduser lists will be added: EnduserList and EnduserList2
            await PatchEnduserPermissionsAsync(documentId,
                new[]
                {
                    new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                    new PermissionDto { Id = Enduser3.Id, Permission = Permission.View }
                });

            var selectedLists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Equal(2, selectedLists.TotalCount);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser.ListId);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser3.ListId);
        }

        [Fact]
        public async Task UploadDocumentEnduserPermissions_ThereAresNewSelectedEnduserLists()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            // Two enduser lists will be added: EnduserList and EnduserList2
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "", "", "", "" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "View" },
                new[] { $"{Enduser3.Id}", "Enduser3", "Enduser 3", "View" }
            });
            await UploadEnduserPermissionsAsync(documentId, excel);

            var selectedLists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Equal(2, selectedLists.TotalCount);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser.ListId);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser3.ListId);
        }

        [Fact]
        public async Task PutDocumentEnduserListPermissions_ThereIsNewSelectedEnduserList()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PutEnduserListPermissionAsync(documentId,
                new PermissionDto { Id = Enduser.ListId, Permission = Permission.View });

            var selectedLists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Equal(1, selectedLists.TotalCount);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser.ListId);
        }

        [Fact]
        public async Task PutSelectedEnduserLists_ValidResult()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PutSelectedEnduserListAsync(documentId, EnduserList.Id);

            var selectedLists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Equal(1, selectedLists.TotalCount);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser.ListId);
            Assert.Equal(EnduserList.Name, selectedLists.Items[0].Name);
        }

        [Fact]
        public async Task PutSelectedEnduserLists_Twice_TheSameResult()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PutSelectedEnduserListAsync(documentId, EnduserList.Id);
            await PutSelectedEnduserListAsync(documentId, EnduserList.Id);

            var selectedLists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Equal(1, selectedLists.TotalCount);
            Assert.Contains(selectedLists.Items, l => l.Id == Enduser.ListId);
            Assert.Equal(EnduserList.Name, selectedLists.Items[0].Name);
        }

        [Fact]
        public async Task DeleteSelectedEnduserList_ThereAreNoSelectedEnduserLists()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PutSelectedEnduserListAsync(documentId, EnduserList.Id);
            await DeleteSelectedEnduserListsAsync(documentId, EnduserList.Id);

            var selectedLists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Empty(selectedLists.Items);
        }

        [Fact]
        public async Task DeleteSelectedEnduserList_ThereAreNoIndividualPermissions()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PatchEnduserPermissionsAsync(documentId,
                new[]
                {
                    new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                    new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
                });

            await DeleteSelectedEnduserListsAsync(documentId, EnduserList.Id);

            var page = await GetEnduserPermissionsAsync(documentId);

            Assert.Equal(0, page.TotalCount);
        }

        [Fact]
        public async Task DeleteSelectedEnduserList_ThereAreNoEnduserLists()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PutEnduserListPermissionAsync(documentId,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            await DeleteSelectedEnduserListsAsync(documentId, EnduserList.Id);

            var lists = await GetSelectedEnduserListsAsync(documentId);

            Assert.Empty(lists.Items);
        }
    }
}
