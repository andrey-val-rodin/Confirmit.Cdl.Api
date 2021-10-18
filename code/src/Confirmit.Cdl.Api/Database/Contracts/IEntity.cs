using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Contracts
{
    [PublicAPI]
    public interface IEntity
    {
        long Id { get; }
    }
}
