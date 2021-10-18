using Confirmit.Cdl.Api.Database.Contracts;
using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class EnduserPermission : IUserPermission
    {
        public long DocumentId { get; set; }

        public int UserId { get; set; }

        public byte Permission { get; set; }
    }
}
