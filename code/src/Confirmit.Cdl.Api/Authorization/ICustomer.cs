using Confirmit.Cdl.Api.Database.Model;
using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Authorization
{
    [PublicAPI]
    public interface ICustomer : IUser
    {
        IAccessor<Document> DocumentAccessor { get; }
    }
}