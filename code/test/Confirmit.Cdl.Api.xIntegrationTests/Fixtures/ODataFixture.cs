using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class ODataFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;
        public string Name;
        public DocumentDto Doc1;
        public DocumentDto Doc2;
        public DocumentDto Doc3;
        public DocumentDto Doc4;
        public DocumentDto DeletedDoc;
        public readonly List<AliasDto> AllAliases = new List<AliasDto>();

        public ODataFixture(SharedFixture sharedFixture)
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

            Name = Guid.NewGuid().ToString();
            var client = new CdlServiceClient(scope);

            await _sharedFixture.UseAdminAsync(scope);

            var name = Name + "doc1";
            Doc1 = await client.PostDocumentAsync(new DocumentToCreateDto
                { Name = name, Type = DocumentType.DataFlow });
            await client.PostRevisionAsync(Doc1.Id, new RevisionToCreateDto());
            await client.PatchEnduserPermissionsAsync(Doc1.Id,
                new[]
                {
                    new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View }
                });
            await client.PatchUserPermissionsAsync(Doc1.Id,
                new[]
                {
                    new UserPermissionDto { Id = _sharedFixture.NormalUser.Id, Permission = Permission.View }
                });

            await _sharedFixture.UseNormalUserAsync(scope);
            name = Name + "doc2";
            Doc2 = await client.PostDocumentAsync(new DocumentToCreateDto
                { Name = name, Type = DocumentType.DataFlow });
            name = Name + "doc3";
            Doc3 = await client.PostDocumentAsync(new DocumentToCreateDto
                { Name = name, Type = DocumentType.DataFlow });

            await _sharedFixture.UseAdminAsync(scope);
            await client.PostRevisionAsync(Doc2.Id, new RevisionToCreateDto());
            await client.PatchEnduserPermissionsAsync(Doc2.Id,
                new[]
                {
                    new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View }
                });
            name = Name + "doc4";
            Doc4 = await client.PostDocumentAsync(new DocumentToCreateDto { Name = name });
            // Access Doc2. This will change Accessed field
            await client.GetDocumentAsync(Doc2.Id);
            // Access Doc3. This will change Accessed field
            await client.GetDocumentAsync(Doc3.Id);
            name = Name + "deleted";
            DeletedDoc = await client.PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await client.DeleteDocumentAsync(DeletedDoc.Id);

            // Append other documents with aliases
            var id = await AddDocumentWithAliasAsync(client, "ons_a", "red");
            // First document will contain two aliases
            AllAliases.Add(await client.PostAliasAsync(new AliasToCreateDto
            {
                Namespace = "ons_a",
                Alias = "orange",
                DocumentId = id
            }));
            await AddDocumentWithAliasAsync(client, "ons_a", "green");
            await AddDocumentWithAliasAsync(client, "ons_a", "blue");
            await AddDocumentWithAliasAsync(client, "ons_a", "gray");
            await AddDocumentWithAliasAsync(client, "ons_b", "green");
            await AddDocumentWithAliasAsync(client, "ons_b", "yellow");

            await _sharedFixture.UseEnduserAsync(scope);
            // Access revision. This will change Accessed field
            await client.GetPublishedRevisionAsync(Doc1.Id);
        }

        private async Task<long> AddDocumentWithAliasAsync(CdlServiceClient client, string @namespace, string alias)
        {
            var id = (await client.PostDocumentAsync()).Id;
            var aliasToCreate = new AliasToCreateDto
            {
                Namespace = @namespace,
                Alias = alias,
                DocumentId = id
            };

            AllAliases.Add(await client.PostAliasAsync(aliasToCreate));

            return id;
        }
    }
}
