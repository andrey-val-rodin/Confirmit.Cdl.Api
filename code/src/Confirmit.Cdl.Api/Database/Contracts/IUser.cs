using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Contracts
{
    [PublicAPI]
    public interface IUser : IEntity
    {
        string Name { get; }
        string FirstName { get; }
        string LastName { get; }
        int OrganizationId { get; }
    }
}
