using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class RevisionFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;

        public DocumentDto Doc1;
        public RevisionDto Doc1Rev1;
        public RevisionDto Doc1Rev2;
        public RevisionDto Doc1Rev3;
        public DocumentDto Doc2;
        public RevisionDto Doc2Rev1;
        public RevisionDto Doc2Rev2;
        public DocumentDto Doc3;

        public RevisionFixture(SharedFixture sharedFixture)
        {
            _sharedFixture = sharedFixture;
        }

        protected override void AddLocalServices(IServiceCollection services)
        {
            var uri = GetServiceUri();
            services.AddConfirmitClient<ICdl>(uri);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var scope = CreateScope();
            var client = new CdlServiceClient(scope);

            await _sharedFixture.UseAdminAsync(scope);

            Doc1 = await client.PostDocumentAsync(new DocumentToCreateDto { Name = "RevisionTests.doc1" });
            await client.PatchEnduserPermissionsAsync(Doc1.Id, new[]
                { new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View } });
            Doc1Rev1 = await client.PostRevisionAsync(Doc1.Id, new RevisionToCreateDto
            {
                Name = "RevisionTests.doc1.rev1",
                SourceCode = "RevisionTests.doc1.rev1"
            });
            Doc1Rev2 = await client.PostRevisionAsync(Doc1.Id, new RevisionToCreateDto
            {
                Name = "RevisionTests.doc1.rev2",
                SourceCode = "RevisionTests.doc1.rev2"
            });
            Doc1Rev3 = await client.PostRevisionAsync(Doc1.Id, new RevisionToCreateDto
            {
                Name = "RevisionTests.doc1.rev3",
                SourceCode = "RevisionTests.doc1.rev3"
            });

            await _sharedFixture.UseNormalUserAsync(scope);
            Doc2 = await client.PostDocumentAsync(new DocumentToCreateDto
            {
                Name = "RevisionTests.doc2",
                Type = DocumentType.ReportalIntegrationDashboard
            });
            await client.PatchUserPermissionsAsync(Doc2.Id, new[]
                { new UserPermissionDto { Id = _sharedFixture.ProsUser.Id, Permission = Permission.View } });

            await _sharedFixture.UseAdminAsync(scope);
            await client.PatchEnduserPermissionsAsync(Doc2.Id, new[]
                { new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View } });
            Doc2Rev1 = await client.PostRevisionAsync(Doc2.Id, new RevisionToCreateDto
            {
                Name = "RevisionTests.doc2.rev1",
                SourceCode = "RevisionTests.doc2.rev1"
            });
            Doc2Rev2 = await client.PostRevisionAsync(Doc2.Id, new RevisionToCreateDto
            {
                Name = "RevisionTests.doc2.rev2",
                SourceCode = "RevisionTests.doc2.rev1"
            });

            Doc3 = await client.PostDocumentAsync(new DocumentToCreateDto { Name = "RevisionTests.doc3" });
            await client.PatchEnduserPermissionsAsync(Doc3.Id, new[]
                { new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View } });
        }
    }
}
