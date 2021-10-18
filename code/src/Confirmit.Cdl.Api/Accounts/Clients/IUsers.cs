using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Accounts.Clients
{
    public interface IUsers
    {
        [Get("/{userId}")]
        Task<HttpResponseMessage> GetUserAsync(int userId,
            [Header("Authorization")] string authorization);

        [Get("")]
        Task<HttpResponseMessage> GetUserAsync(string userKey,
            [Header("Authorization")] string authorization);

        [Get("")]
        Task<HttpResponseMessage> GetUsersAsync(int companyId,
            [Header("Authorization")] string authorization);
    }
}
