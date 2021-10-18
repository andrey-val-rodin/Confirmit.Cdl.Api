using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Contracts
{
    [PublicAPI]
    public interface IOrganization : IEntity
    {
        string Name { get; }
    }
}
