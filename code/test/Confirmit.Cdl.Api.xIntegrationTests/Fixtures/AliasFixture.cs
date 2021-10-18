using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class AliasFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;
        public long DocumentId;
        public long DocumentWith2AliasesId;
        public AliasDto Alias;

        public AliasFixture(SharedFixture sharedFixture)
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

            // Create document and published revision
            DocumentId = (await client.PostDocumentAsync()).Id;
            await client.PostRevisionAsync(DocumentId);

            // Grant permission View to NormalUser
            await client.PatchUserPermissionsAsync(DocumentId,
                new[] { new UserPermissionDto { Id = _sharedFixture.NormalUser.Id, Permission = Permission.View } });
            // Grant permission View to Enduser
            await client.PatchEnduserPermissionsAsync(DocumentId,
                new[] { new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View } });

            // Create alias
            // At First, ensure alias doesn't exist yet
            await client.GetAliasAsync(DocumentId, HttpStatusCode.NotFound);
            Alias = await client.PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "aliases-tests",
                Alias = "my_doc",
                DocumentId = DocumentId
            });

            // Append other documents with aliases
            DocumentWith2AliasesId = await AddDocumentWithAliasAsync(client, "ns_a", "red");
            // First document will contain two aliases
            await client.PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "ns_a",
                Alias = "orange",
                DocumentId = DocumentWith2AliasesId
            });
            await AddDocumentWithAliasAsync(client, "ns_a", "green");
            await AddDocumentWithAliasAsync(client, "ns_a", "blue");
            await AddDocumentWithAliasAsync(client, "ns_a", "gray");
            await AddDocumentWithAliasAsync(client, "ns_b", "green");
            await AddDocumentWithAliasAsync(client, "ns_b", "yellow");
        }

        private static async Task<long> AddDocumentWithAliasAsync(
            CdlServiceClient client, string @namespace, string alias)
        {
            var id = (await client.PostDocumentAsync()).Id;
            var aliasToCreate = new AliasToCreateDto
            {
                Namespace = @namespace,
                Alias = alias,
                DocumentId = id
            };

            await client.PostAliasAsync(aliasToCreate);

            return id;
        }
    }
}
