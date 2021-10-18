using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    public class HealthzTests : IntegrationTestBase<HealthzFixture>, IClassFixture<HealthzFixture>
    {
        public HealthzTests(HealthzFixture fixture, ITestOutputHelper outputHelper) : base(outputHelper, fixture)
        {
        }

        [Fact]
        public async void Ready()
        {
            using var scope = Fixture.CreateScope();

            var healthz = scope.GetService<IHealthz>();
            var response = await healthz.GetReady();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async void Live()
        {
            using var scope = Fixture.CreateScope();

            var healthz = scope.GetService<IHealthz>();
            var response = await healthz.GetLive();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
