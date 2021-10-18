using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using Refit;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public sealed class EnduserList : IAsyncDisposable
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private EnduserList()
        {
        }

        public static async Task<EnduserList> GetOrCreateAsync(SharedFixture fixture, string name)
        {
            using var scope = fixture.CreateScope();

            await fixture.UseAdminAsync(scope);
            var service = scope.GetService<IEndusers>();
            return await FindAsync(service, name) ?? await CreateAsync(service, name);
        }

        private static async Task<EnduserList> FindAsync(IEndusers service, string name)
        {
            try
            {
                return await service.GetEnduserListAsync(name);
            }
            catch (ApiException e)
            {
                Assert.False(HttpStatusCode.InternalServerError == e.StatusCode,
                    "There is more than one enduser list with this name");
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<EnduserList> CreateAsync(IEndusers service, string name)
        {
            var response = await service.CreateEnduserListAsync(new EnduserList { Name = name });
            Assert.True(response.StatusCode == HttpStatusCode.Created, "Unable to create enduser list");

            return await FindAsync(service, name);
        }

        public async ValueTask DisposeAsync()
        {
            await new TestDbWriter().DeleteEnduserListAsync(Id);
        }
    }
}
