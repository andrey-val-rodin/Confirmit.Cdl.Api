using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    public interface IHealthz
    {
        [Get("/healthz/live")]
        Task<HttpResponseMessage> GetLive();

        [Get("/healthz/ready")]
        Task<HttpResponseMessage> GetReady();

        [Get("/healthz/scope")]
        Task<HttpResponseMessage> GetScope();
    }
}