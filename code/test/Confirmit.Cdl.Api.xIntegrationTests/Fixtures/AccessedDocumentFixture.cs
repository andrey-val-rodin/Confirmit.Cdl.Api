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
    public class AccessedDocumentFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;

        public string Name;

        public AccessedDocumentFixture(SharedFixture sharedFixture)
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

            Name = Guid.NewGuid().ToString();

            await _sharedFixture.UseAdminAsync(scope);
            // Create 5 documents and give permission View to user and enduser to all of them
            var documents = new List<DocumentDto>();
            for (int i = 0; i < 5; i++)
            {
                var doc = await client.PostDocumentAsync(new DocumentToCreateDto { Name = Name });
                await client.PatchUserPermissionsAsync(doc.Id, new[]
                {
                    new UserPermissionDto { Id = _sharedFixture.NormalUser.Id, Permission = Permission.View }
                });
                await client.PatchEnduserPermissionsAsync(doc.Id, new[]
                {
                    new PermissionDto { Id = _sharedFixture.Enduser.Id, Permission = Permission.View }
                });

                // Create published revision for endusers
                await client.PostRevisionAsync(doc.Id);

                documents.Add(doc);
            }

            await _sharedFixture.UseNormalUserAsync(scope);

            // User accessed documents #0, #1 and #2
            await client.GetDocumentAsync(documents[0].Id); // Get document (Accessed timestamp will be updated)
            await client.GetDocumentAsync(documents[1].Id);
            await client.GetDocumentAsync(documents[2].Id);

            await _sharedFixture.UseEnduserAsync(scope);
            // Enduser accessed documents #3 and #4
            // Get published revision (Accessed timestamp will be updated)
            await client.GetPublishedRevisionAsync(documents[3].Id);
            await client.GetPublishedRevisionAsync(documents[4].Id);
        }
    }
}
