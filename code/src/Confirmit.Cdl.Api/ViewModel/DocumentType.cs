using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public enum DocumentType
    {
        NotSpecified = 0,
        ReportingDashboard = 1,
        ReportalIntegrationDashboard = 2,
        DataTemplate = 3,
        DataFlow = 4,
        ProgramDashboard = 5,
        Automation = 6,
        ReportingTemplate = 7,
        HubDataFlow = 8
    }
}