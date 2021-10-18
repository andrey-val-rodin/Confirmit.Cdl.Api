using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PermissionStub
    {
        public int Id { get; set; }
        public string Permission { get; set; }
    }
}