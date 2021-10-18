using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    public interface IUsers
    {
        [Get("")]
        Task<User.InternalUsers> GetUsersAsync(int companyId);

        [Post("/normal")]
        Task<HttpResponseMessage> CreateUserAsync([Body] User user);

        [Post("/{userId}/permissions")]
        Task<HttpResponseMessage> SetPermission(
            int userId, [Body(BodySerializationMethod.Serialized)] string permission);
    }
}
