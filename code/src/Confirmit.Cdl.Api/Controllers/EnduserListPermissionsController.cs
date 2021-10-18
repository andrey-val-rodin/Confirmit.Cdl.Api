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
    /// Controller for handling enduser permissions on the specified document
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class EnduserListPermissionsController : ControllerBase
    {
        public EnduserListPermissionsController(EnduserPermissionService service)
        {
            PermissionService = service;
        }

        private EnduserPermissionService PermissionService { get; }

        /// <summary>
        /// Returns all enduser lists whose endusers have access to the document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <remarks>
        /// Response contains enduser lists from both endusers and enduser lists permissions.
        /// </remarks>
        /// <returns>List of permissions</returns>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/enduserlists", Name = "GetAllEnduserLists")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationDto>))]
        public async Task<IActionResult> GetAllEnduserListsAsync(
            long id,
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            var enduserLists = PermissionService.GetAllOrganizations(id);
            var opts = ODataBuilder.BuildOptions<OrganizationDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(enduserLists, opts);

            return Ok(new PageDto<OrganizationDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetAllEnduserLists", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetAllEnduserLists", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Returns enduser lists permissions for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// You can use this endpoint to get all enduser lists with 'whole' permission.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/enduserlistpermissions", Name = "GetEnduserListPermissions")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationPermissionDto>))]
        public async Task<IActionResult> GetEnduserListPermissionsAsync(
            long id,
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            var permissions = PermissionService.GetOrganizationPermissions(id);
            var opts = ODataBuilder.BuildOptions<OrganizationPermissionDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(permissions, opts);

            return Ok(new PageDto<OrganizationPermissionDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetEnduserListPermissions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetEnduserListPermissions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Sets permission for the whole enduser list
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="permission">contains Id of enduser list and permission "View" or "None"</param>
        /// <response code="200">OK</response>
        /// <response code="400">Validation error (see error message)</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or enduser list not found</response>
        [Route("{id:long}/enduserlistpermissions", Name = "PutEnduserListPermission")]
        [HttpPut]
        public async Task<IActionResult> PutEnduserListPermissionAsync(
            long id,
            [FromBody] PermissionDto permission)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.ValidatePermissionAsync(permission.Id, permission.Permission);

            await PermissionService.SetEnduserListPermissionAsync(id, permission.Id, permission.Permission);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }

        /// <summary>
        /// Deletes permission of the whole enduser list
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserListId">Id of the enduser list to delete</param>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or permission not found</response>
        [Route("{id:long}/enduserlistpermissions/{enduserListId:int}", Name = "DeleteEnduserListPermission")]
        [HttpDelete]
        public async Task<IActionResult> DeleteEnduserListPermissionAsync(
            long id,
            int enduserListId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await PermissionService.DeleteEnduserListPermissionAsync(id, enduserListId))
                return NotFound($"Enduser list {enduserListId} not found");

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }
    }
}
