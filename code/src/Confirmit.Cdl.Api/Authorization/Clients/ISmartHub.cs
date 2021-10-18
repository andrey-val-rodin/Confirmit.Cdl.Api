using Refit;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization.Clients
{
    [Headers("Accept: application/json")]
    public interface ISmartHub
    {
        [Get("/hubs/{hubId}/access")]
        Task<HubAccess> GetPermission(long hubId);
    }
}
