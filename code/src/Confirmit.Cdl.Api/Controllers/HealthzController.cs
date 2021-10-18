//==== DO NOT MODIFY THIS FILE ====
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Confirmit.Cdl.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize("DefaultAccessPolicy")]
    public class HealthzController : ControllerBase
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<HealthzController> _logger;

        public HealthzController(ILogger<HealthzController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns 200 when the service is ready to accept traffic. Any other return value indicates that the service should stop receiving traffic. This endpoint can also be used for warmup
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("healthz/ready")]
        [HttpGet]
        public IActionResult ReadinessProbe()
        {
            //TODO: Check that service is ready to receive traffic
            return Ok("Ready!");
        }

        /// <summary>
        /// Returns 200 when the service is healthy. If service is not healthy returns 500 to signal that the service should be restarted
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("healthz/live")]
        [HttpGet]
        public IActionResult LivenessProbe()
        {
            //TODO: Check that service is still healthy or if the pod needs to be restarted
            return Ok("Live!");
        }

        /// <summary>
        /// Returns 200 when the service scope is setup correctly
        /// </summary>
        /// <returns></returns>
        [Route("healthz/scope")]
        [HttpGet]
        public IActionResult ScopeProbe()
        {
            return Ok("Scope is ok!");
        }
    }
}
