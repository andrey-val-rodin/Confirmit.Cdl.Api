using Confirmit.Cdl.Api.Authorization.Clients;
using Confirmit.NetCore.Identity.Sdk.Claims;
using Confirmit.NetCore.Identity.Sdk.Clients;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization
{
    /// <summary>
    /// Requests user's survey access from Metadata service
    /// </summary>
    [PublicAPI]
    public sealed class SurveyPermissionReader
    {
        private readonly ILogger<SurveyPermissionReader> _logger;
        private readonly ClaimsPrincipal _principal;
        private readonly IMetadata _metadata;
        private readonly IConfirmitTrustedClient _trustedClient;

        public SurveyPermissionReader(
            ILogger<SurveyPermissionReader> logger,
            IHttpContextAccessor httpContext,
            IMetadata metadata,
            IConfirmitTrustedClient trustedClient)
        {
            _logger = logger;
            _principal = httpContext.HttpContext.User;
            _metadata = metadata;
            _trustedClient = trustedClient;
        }

        public async Task<Permission> GetPermissionAsync(string surveyId)
        {
            // Endusers do not have explicit permissions to survey
            if (_principal.IsEndUser())
                return Permission.None;

            const string scope = "metadata";
            try
            {
                var response = await _trustedClient.InvokeAsync(scope, () => _metadata.GetProject(surveyId))
                    .ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                    return Permission.None;

                // If Http code is OK, assume that current user has read access at least
                return Permission.View;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception during obtaining permission to survey {surveyId}", e);
                return Permission.None;
            }
        }
    }
}