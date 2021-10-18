using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Authorization.Clients
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HubAccess
    {
        public bool View { get; set; }
        public bool Manage { get; set; }
    }
}
