using Confirmit.Cdl.Api.Database.Contracts;

namespace Confirmit.Cdl.Api.Authorization
{
    public class PermittedResource<T> where T : class, IEntity
    {
        public T Resource { get; set; }
        public Permission Permission { get; set; }
    }
}
