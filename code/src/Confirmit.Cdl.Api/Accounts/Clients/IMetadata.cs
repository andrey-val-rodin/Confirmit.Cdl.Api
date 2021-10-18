using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Accounts.Clients
{
    [Headers("Accept: application/json")]
    public interface IMetadata
    {
        [Get("/companies/{companyId}")]
        Task<HttpResponseMessage> GetCompanyAsync(int companyId,
            [Header("Authorization")] string authorization);

        [Get("/companies")]
        Task<HttpResponseMessage> GetCompaniesAsync(
            [AliasAs("$skip")] int skip,
            [AliasAs("$top")] int top,
            int permission,
            [Header("Authorization")] string authorization);
    }
}
