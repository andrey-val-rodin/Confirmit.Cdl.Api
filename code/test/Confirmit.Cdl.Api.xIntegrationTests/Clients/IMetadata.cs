using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Newtonsoft.Json.Linq;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    [Headers("Accept: application/json")]
    public interface IMetadata
    {
        [Get("/companies")]
        Task<Company> GetCompanyAsync(string name);

        [Post("/companies")]
        Task<HttpResponseMessage> CreateCompanyAsync([Body] Company company);

        [Get("/projects")]
        Task<Survey[]> GetProjectsAsync([AliasAs("$filter")] string filter);

        [Post("/projects")]
        Task<HttpResponseMessage> CreateProjectAsync(string projectType);

        [Patch("/projects/{projectId}/ProjectInfo")]
        Task<HttpResponseMessage> SetProjectName(string projectId, [Body] JObject name);
    }
}
