using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization.Clients
{
    [Headers("Accept: application/json")]
    public interface IMetadata
    {
        [Get("/projects/{surveyId}")]
        Task<HttpResponseMessage> GetProject(string surveyId);
    }
}
