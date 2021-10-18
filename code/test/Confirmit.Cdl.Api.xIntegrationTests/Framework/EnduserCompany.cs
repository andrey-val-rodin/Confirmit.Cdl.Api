using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public class EnduserCompany : IAsyncDisposable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ListId { get; set; }

        private EnduserCompany()
        {
        }

        public static async Task<EnduserCompany> GetOrCreateAsync(
            SharedFixture fixture, EnduserList enduserList, string name)
        {
            using var scope = fixture.CreateScope();

            await fixture.UseAdminAsync(scope);
            var service = scope.GetService<IEndusers>();
            return await FindAsync(service, enduserList, name) ??
                   await CreateAsync(service, enduserList, name);
        }

        private static async Task<EnduserCompany> FindAsync(IEndusers service, EnduserList enduserList, string name)
        {
            InternalCompanies items;
            try
            {
                items = await service.GetEnduserCompaniesAsync(enduserList.Id);
            }
            catch (Exception)
            {
                items = null;
            }

            var companies = items?.Items;
            return companies?.SingleOrDefault(c => c.Name == name);
        }

        private static async Task<EnduserCompany> CreateAsync(
            IEndusers service, EnduserList enduserList, string name)
        {
            var response =
                await service.CreateEnduserCompanyAsync(new EnduserCompany { Name = name, ListId = enduserList.Id });
            Assert.True(response.StatusCode == HttpStatusCode.Created, "Unable to create enduser company");

            return await FindAsync(service, enduserList, name);
        }

        public async ValueTask DisposeAsync()
        {
            await new TestDbWriter().DeleteEnduserCompanyAsync(Id, ListId);
        }

        public class InternalCompanies
        {
            [UsedImplicitly]
            public EnduserCompany[] Items { get; set; }
        }
    }
}
