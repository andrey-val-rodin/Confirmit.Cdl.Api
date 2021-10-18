using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Accounts.Clients
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
