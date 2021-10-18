using Confirmit.Cdl.Api.Authorization.Clients;
using Confirmit.NetCore.Identity.Sdk.Claims;
using Confirmit.NetCore.Identity.Sdk.Clients;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Authorization
{
    /// <summary>
    /// Requests user's hub access from Smart Hub service
    /// </summary>
    [PublicAPI]
    public sealed class HubPermissionReader
    {
        private readonly ILogger<HubPermissionReader> _logger;
        private readonly ClaimsPrincipal _principal;
        private readonly ISmartHub _smartHub;
        private readonly IConfirmitTrustedClient _client;

        public HubPermissionReader(
            ILogger<HubPermissionReader> logger,
            IHttpContextAccessor httpContext,
            ISmartHub smartHub,
            IConfirmitTrustedClient client)
        {
            _logger = logger;
            _principal = httpContext.HttpContext.User;
            _smartHub = smartHub;
            _client = client;
        }

        public async Task<Permission> GetPermissionAsync(long hubId)
        {
            // Endusers do not have explicit permissions to hub
            if (_principal.IsEndUser())
                return Permission.None;

            const string scope = "smarthub";
            try
            {
                var access = await _client.InvokeAsync(scope, () => _smartHub.GetPermission(hubId))
                    .ConfigureAwait(false);

                return access.Manage ? Permission.Manage
                    : access.View ? Permission.View
                    : Permission.None;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception during obtaining permission to hub {hubId}");
                return Permission.None;
            }
        }
    }
}