using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.NetCore.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Middleware
{
    /// <summary>
    /// Refreshes current principal.
    /// Refreshing means inserting the user into internal DB or updating it in DB
    /// if some fields of user or its organization were changed.
    /// RefreshUserFilter class performs refreshing task in separate thread.
    /// </summary>
    public class RefreshUserFilter : IActionFilter
    {
        private readonly ILogger<RefreshUserFilter> _logger;
        private readonly IAccountLoader _accountLoader;

        public RefreshUserFilter(ILogger<RefreshUserFilter> logger, IAccountLoader accountLoader)
        {
            _logger = logger;
            _accountLoader = accountLoader;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var principal = actionContext.HttpContext.User;
            if (!Factory.IsPrincipalValid(principal))
                return;

            if (!actionContext.HttpContext.Items.ContainsKey(typeof(SuppressItemKey)))
            {
                new DbRefresher(_accountLoader, principal).RefreshAsync()
                    .ContinueWith(
                        action =>
                        {
                            _logger.ErrorException(action.Exception, "Error during refreshing current principal in DB.");
                        }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public static void Suppress(HttpContext context)
        {
            context.Items[typeof(SuppressItemKey)] = null;
        }

        private class SuppressItemKey
        {
        }
    }
}
