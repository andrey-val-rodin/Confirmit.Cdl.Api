using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    public interface ISwagger
    {
        [Get("/swagger/index.html")]
        Task<HttpResponseMessage> Get();
    }
}