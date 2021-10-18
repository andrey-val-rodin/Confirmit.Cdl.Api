using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class ExcelFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;
        public long DocumentId;

        public ExcelFixture(SharedFixture sharedFixture)
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

            DocumentId = (await client.PostDocumentAsync()).Id;
        }
    }
}
