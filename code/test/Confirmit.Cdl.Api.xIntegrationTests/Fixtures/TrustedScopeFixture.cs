using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class TrustedScopeFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;
        public DocumentDto Document;

        public TrustedScopeFixture(SharedFixture sharedFixture)
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

            // Create document as Admin; NormalUser won't have explicit permissions to it
            var name = Guid.NewGuid().ToString();
            Document = await client.PostDocumentAsync(new DocumentToCreateDto
                { Name = name, Type = DocumentType.DataTemplate });
        }
    }
}
