using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class SwaggerFixture : BaseFixture
    {
        protected override void AddLocalServices(IServiceCollection services)
        {
            var uri = GetServiceUri();
            services.AddConfirmitClient<ISwagger>(uri);
        }
    }
}