using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Tools
{
    /// <summary>
    /// POCO object to read values from appsettings.json, section Confirmit/Cleanup
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CleanupConfig
    {
        public int ExpirationPeriodInDays { get; set; } = 30;
        public int CleanupIntervalInMinutes { get; set; } = 240;
    }
}
