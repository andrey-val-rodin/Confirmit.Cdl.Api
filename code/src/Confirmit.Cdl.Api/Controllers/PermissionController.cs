using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Controllers
{
    /// <summary>
    /// Controller for handling user permissions on the specified document
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class PermissionController : ControllerBase
    {
        public PermissionController(DocumentService documentService)
        {
            DocumentService = documentService;
        }

        private DocumentService DocumentService { get; }

        /// <summary>
        /// Returns permission of the current user to the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>Permission</returns>
        /// <remarks>
        /// If the specified document does not exist, response code will be 404 Not Found.
        /// If current user has not permission, response code will be 403 Forbidden.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/permission", Name = "GetDocumentPermission")]
        [HttpGet]
        [Produces(typeof(Permission))]
        public async Task<IActionResult> GetDocumentPermissionAsync(
            long id)
        {
            var permission = await DocumentService.GetPermissionAsync(id, ResourceStatus.Exists);
            if (permission == Permission.None)
                return Forbid();

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, DocumentService.GetTimestamp());

            return Ok(permission);
        }
    }
}
