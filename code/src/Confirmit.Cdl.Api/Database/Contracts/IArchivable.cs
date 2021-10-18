using JetBrains.Annotations;
using System;

namespace Confirmit.Cdl.Api.Database.Contracts
{
    [PublicAPI]
    public interface IArchivable : IEntity
    {
        DateTime? Deleted { get; }
    }
}
