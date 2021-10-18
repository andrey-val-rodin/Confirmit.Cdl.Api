using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public sealed class Company : IAsyncDisposable
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private Company()
        {
        }

        public static async Task<Company> GetOrCreateAsync(SharedFixture fixture, string name)
        {
            using var scope = fixture.CreateScope();

            await fixture.UseAdminAsync(scope);
            var service = scope.GetService<IMetadata>();
            return await FindAsync(service, name) ?? await CreateAsync(service, name);
        }

        private static async Task<Company> FindAsync(IMetadata service, string name)
        {
            try
            {
                return await service.GetCompanyAsync(name);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Company> CreateAsync(IMetadata service, string name)
        {
            var response = await service.CreateCompanyAsync(new Company { Name = name });
            Assert.True(response.StatusCode == HttpStatusCode.Created, "Unable to create company");

            return await FindAsync(service, name);
        }

        public async ValueTask DisposeAsync()
        {
            await new TestDbWriter().DeleteCompanyAsync(Id);
        }
    }
}
