using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using System;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    public abstract class BaseFixture : FixtureBase
    {
        public override string Scope => "";

        public static Uri GetServiceUri()
        {
            var developmentHostname = Environment.GetEnvironmentVariable("Confirmit__Development__Host");
            var environmentVariable = Environment.GetEnvironmentVariable("Confirmit__Deployment__Hostname");

            if (!string.IsNullOrWhiteSpace(developmentHostname))
                return new Uri(developmentHostname);

            return !string.IsNullOrWhiteSpace(environmentVariable)
                ? new Uri($"http://{environmentVariable}/api/cdl")
                : new Uri("http://localhost/api/cdl");
        }
    }
}
