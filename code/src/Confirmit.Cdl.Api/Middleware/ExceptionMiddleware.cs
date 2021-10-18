using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Middleware
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(
                        "The response has already started, exception middleware will not be executed.");
                    throw;
                }

                await OnException(context, e);
            }
        }

        private async Task OnException(HttpContext context, Exception exception)
        {
            HttpStatusCode code;
            string message;
            switch (exception)
            {
                case SecurityException _:
                    _logger.LogError(exception, "Authorization exception.");
                    code = HttpStatusCode.Unauthorized;
                    message = exception.Message;
                    break;

                case BadRequestException _:
                case ODataException _:
                    code = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                case NotFoundException _:
                    code = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case DeadlockException _:
                    if (exception.InnerException != null)
                        _logger.LogError(exception.InnerException, exception.Message);
                    code = HttpStatusCode.Conflict;
                    message = exception.Message;
                    break;

                case DbUpdateConcurrencyException _:
                    _logger.LogError(exception, "Database concurrency violation.");
                    code = HttpStatusCode.Conflict;
                    message = exception.Message;
                    break;

                case DbUpdateException _:
                case SqlException _:
                    _logger.LogError(exception, "Database error.");
                    code = HttpStatusCode.Conflict;
                    message = exception.Message;
                    break;

                default:
                    _logger.LogError(exception, "Unexpected error.");
                    code = HttpStatusCode.InternalServerError;
                    message = exception.Message;
                    break;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int) code;
            if (!string.IsNullOrEmpty(message))
            {
                context.Response.ContentType = "application/json";
                var result = JsonConvert.SerializeObject(new { message });
                await context.Response.WriteAsync(result);
            }
        }
    }
}
