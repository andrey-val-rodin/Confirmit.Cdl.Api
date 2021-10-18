using Confirmit.NetCore.Common;
using System;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    public class ConfirmitScopeContextStub : IConfirmitScopeContext
    {
        public string CorrelationId { get; set; } = "Confirmit.Cdl.Api.IntegrationTests-" + Guid.NewGuid();
        public string InitiatingService { get; set; } = "Confirmit.Cdl.Api.IntegrationTests";
        public string ReferrerService { get; set; } = "Confirmit.Cdl.Api.IntegrationTests";
        public string ApplicationName { get; } = "Confirmit.Cdl.Api.IntegrationTests";
    }
}