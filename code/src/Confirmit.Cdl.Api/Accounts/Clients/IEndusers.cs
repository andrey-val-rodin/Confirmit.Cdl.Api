using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Accounts.Clients
{
    public interface IEndusers
    {
        [Get("/users/{enduserId}")]
        Task<HttpResponseMessage> GetEnduserAsync(int enduserId,
            [Header("Authorization")] string authorization);

        [Get("/lists/{listId}")]
        Task<HttpResponseMessage> GetEnduserListAsync(int listId,
            [Header("Authorization")] string authorization);

        [Get("/companies/{companyId}")]
        Task<HttpResponseMessage> GetEnduserCompanyAsync(int companyId,
            [Header("Authorization")] string authorization);

        [Get("/companies")]
        Task<HttpResponseMessage> GetEnduserListCompaniesAsync(int listId,
            [Header("Authorization")] string authorization);

        [Get("/users")]
        Task<HttpResponseMessage> GetEndusersInListAsync(int listId,
            [Header("Authorization")] string authorization);
    }
}
