using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Controllers
{
    /// <summary>
    /// Controller for handling selected enduser lists
    /// </summary>
    [ApiController]
    [Route("documents/{id:long}/enduserpermissions/enduserlists/selected")]
    [Authorize("DefaultAccessPolicy")]
    public class SelectedEnduserListsController : ControllerBase
    {
        public SelectedEnduserListsController(EnduserPermissionService service)
        {
            PermissionService = service;
        }

        private EnduserPermissionService PermissionService { get; }

        /// <summary>
        /// Returns selected enduser lists for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Selected enduser lists are lists that you can attach to a document.
        /// The service keeps them even without actual permissions.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("", Name = "GetSelectedEnduserLists")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationDto>))]
        public async Task<IActionResult> GetSelectedEnduserListsAsync(
            long id,
#pragma warning disable IDE0060 // Remove unused parameter
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            var enduserLists = PermissionService.GetSelectedEnduserLists(id);
            var opts = ODataBuilder.BuildOptions<OrganizationDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(enduserLists, opts);

            return Ok(new PageDto<OrganizationDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetSelectedEnduserLists", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetSelectedEnduserLists", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Appends the specified enduser list to the document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserListId">Enduser list Id</param>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">Document or enduser list with the specified Id not found or current user has not required permissions</response>
        [Route("", Name = "PutDocumentSelectedEnduserLists")]
        [HttpPut]
        public async Task<IActionResult> PutDocumentSelectedEnduserListsAsync(
            long id,
            [FromBody] int enduserListId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.ValidatePermissionAsync(enduserListId, Permission.View);
            await PermissionService.InsertSelectedEnduserListAsync(id, enduserListId);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }

        /// <summary>
        /// Deletes the specified enduser list
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserListId">Id of the enduser list to delete</param>
        /// <remarks>All actual permissions will be deleted as well.</remarks>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">Document or enduser list with the specified Id not found or current user has not required permissions</response>
        [Route("{enduserListId:int}", Name = "DeleteDocumentSelectedEnduserList")]
        [HttpDelete]
        public async Task<IActionResult> DeleteDocumentSelectedEnduserListAsync(
            long id,
            int enduserListId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.DeleteSelectedEnduserListAsync(id, enduserListId);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }
    }
}
