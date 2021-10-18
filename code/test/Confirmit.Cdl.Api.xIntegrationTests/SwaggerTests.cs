using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Configuration;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    public class SwaggerTests : IntegrationTestBase<SwaggerFixture>, IClassFixture<SwaggerFixture>
    {

        public SwaggerTests(SwaggerFixture fixture, ITestOutputHelper outputHelper) : base(outputHelper, fixture)
        {
        }

        [Fact]
        public async void Ok()
        {
            if (!ConfirmitConfiguration.ServiceDocumentationEnabled)
                return;

            using var scope = Fixture.CreateScope();

            var swagger = scope.GetService<ISwagger>();
            var response = await swagger.Get();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}