using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Accounts
{
    public class User
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; [UsedImplicitly] set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}