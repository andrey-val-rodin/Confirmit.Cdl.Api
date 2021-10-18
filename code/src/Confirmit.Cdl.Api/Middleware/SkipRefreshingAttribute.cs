using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Confirmit.Cdl.Api.Middleware
{
    public class SkipRefreshingAttribute : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order { get; set; }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            RefreshUserFilter.Suppress(context.HttpContext);
        }
    }
}
