using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    public interface IEndusers
    {
        [Get("/lists/{name}")]
        Task<EnduserList> GetEnduserListAsync(string name);

        [Post("/lists")]
        Task<HttpResponseMessage> CreateEnduserListAsync([Body] EnduserList list);

        [Get("/companies")]
        Task<EnduserCompany.InternalCompanies> GetEnduserCompaniesAsync(int listId);

        [Post("/companies")]
        Task<HttpResponseMessage> CreateEnduserCompanyAsync([Body] EnduserCompany company);

        [Get("/users")]
        Task<Enduser.InternalUsers> GetEndusersAsync(int listId);

        [Post("/users")]
        Task<HttpResponseMessage> CreateEnduserAsync([Body] Enduser enduser);
    }
}
