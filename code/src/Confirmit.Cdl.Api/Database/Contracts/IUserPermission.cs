using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Contracts
{
    [PublicAPI]
    public interface IUserPermission
    {
        long DocumentId { get; }
        int UserId { get; }
        byte Permission { get; }
    }
}
