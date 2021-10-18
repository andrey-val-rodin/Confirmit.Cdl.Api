using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    [Headers("Accept: application/json")]
    public interface ISmartHub
    {
        [Get("/hubs")]
        Task<Hub[]> FindAsync(string search);

        [Post("/hubs")]
        Task<HttpResponseMessage> CreateAsync(Hub hub);
    }
}
