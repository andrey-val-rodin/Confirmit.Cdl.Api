using JetBrains.Annotations;
using System.Collections.ObjectModel;

namespace Confirmit.Cdl.Api.Authorization
{
    [PublicAPI]
    public interface IUser
    {
        int Id { get; }
        int OrganizationId { get; }
        UserType UserType { get; }
        ReadOnlyCollection<Role> Roles { get; }
        ReadOnlyCollection<string> Scopes { get; }

        bool IsInRole(Role role);
    }
}