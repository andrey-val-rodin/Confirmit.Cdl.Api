using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public class Hub
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public static async Task<Hub> GetOrCreateAsync(SharedFixture fixture, string name)
        {
            using var scope = fixture.CreateScope();

            await fixture.UseAdminAsync(scope);
            var service = scope.GetService<ISmartHub>();
            return await FindAsync(service, name) ?? await CreateAsync(service, name);
        }

        private static async Task<Hub> FindAsync(ISmartHub service, string name)
        {
            try
            {
                var hubs = await service.FindAsync(name);
                return hubs?.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Hub> CreateAsync(ISmartHub service, string name)
        {
            await service.CreateAsync(new Hub { Name = name });
            return await FindAsync(service, name);
        }
    }
}
