using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Controllers
{
    /// <summary>
    /// Root controller to display links to ResourceCollections in Cdl API
    /// </summary>
    [ApiController]
    [Authorize("DefaultAccessPolicy")]
    public class RootController : ControllerBase
    {
        public RootController(DocumentService service)
        {
            DocumentService = service;
        }

        private DocumentService DocumentService { get; }

        /// <summary>
        /// Displays links to all resource collections in Document API
        /// </summary>
        /// <returns>List of links to the resource collections</returns>
        [Route("", Name = "GetRootInfo")]
        [HttpGet]
        [AllowAnonymous]
        [Produces(typeof(RootDto))]
        public async Task<IActionResult> GetRootInfoAsync()
        {
            var result = new RootDto();
            var customer = User.Identity.IsAuthenticated ? await DocumentService.Customer : null;
            result.FillLinks(customer, Url);
            return customer != null
                ? (IActionResult) Ok(result)
                : Unauthorized(result);
        }
    }
}
