using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Controllers
{
    /// <summary>
    /// Controller for handling enduser permissions on the specified document
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class EnduserPermissionsController : ControllerBase
    {
        public EnduserPermissionsController(EnduserPermissionService service)
        {
            PermissionService = service;
        }

        private EnduserPermissionService PermissionService { get; }

        /// <summary>
        /// Returns enduser permissions for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$filter=EnduserCompanyId eq 1.
        /// </remarks>
        /// <returns>Requested page</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/enduserpermissions", Name = "GetEnduserPermissions")]
        [HttpGet]
        [Produces(typeof(PageDto<EnduserPermissionFullDto>))]
        public async Task<IActionResult> GetEnduserPermissionsAsync(
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

            var permissions = PermissionService.GetPermissions(id);
            var opts = ODataBuilder.BuildOptions<EnduserPermissionFullDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(permissions, opts);

            return Ok(new PageDto<EnduserPermissionFullDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetEnduserPermissions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetEnduserPermissions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Returns all enduser lists of endusers that have access to the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>List of permissions</returns>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/enduserpermissions/enduserlists", Name = "GetEnduserPermissionsEnduserLists")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationDto>))]
        public async Task<IActionResult> GetEnduserPermissionsEnduserListsAsync(
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

            var enduserLists = PermissionService.GetUserPermissionsOrganizations(id);
            var opts = ODataBuilder.BuildOptions<OrganizationDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(enduserLists, opts);

            return Ok(new PageDto<OrganizationDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetEnduserPermissionsEnduserLists", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetEnduserPermissionsEnduserLists", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Returns permissions for all endusers in the specified enduser list
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserListId">Id of enduser list</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$filter=EnduserCompanyId eq 1.
        /// </remarks>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// If enduser has not permission, list will contain such enduser with permission None.
        /// Result contains only individual enduser permissions. Permission of the whole enduser list does not affect the result.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or enduser list not found</response>
        [Route("{id:long}/enduserpermissions/enduserlists/{enduserListId:int}", Name = "GetEnduserPermissionsForEnduserList")]
        [HttpGet]
        [Produces(typeof(PageDto<EnduserPermissionFullDto>))]
        public async Task<IActionResult> GetEnduserPermissionsForEnduserListAsync(
            long id,
            int enduserListId,
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var permissions = await PermissionService.GetEnduserPermissionsAsync(id, enduserListId);
            if (permissions == null)
                return Forbid();

            // Use sync version of ApplyODataQueryOptions because permissions is a native list and
            // doesn't support async operations
            var opts = ODataBuilder.BuildOptions<EnduserPermissionFullDto>(Request);
            var page = BaseService.ApplyODataQueryOptions(permissions.AsQueryable(), opts);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok(new PageDto<EnduserPermissionFullDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(
                        Url, "GetEnduserPermissionsForEnduserList", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(
                        Url, "GetEnduserPermissionsForEnduserList", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Returns permissions for all endusers in enduser list as Excel stream
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserListId">Id of enduser list</param>
        /// <returns>Stream containing Excel document</returns>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or enduser list not found</response>
        [Route("{id:long}/enduserpermissions/enduserlists/{enduserListId:int}/download",
            Name = "DownloadDocumentEnduserPermissionsForEnduserList")]
        [HttpGet]
        public async Task<IActionResult> DownloadEnduserPermissionsForEnduserListAsync(
            long id,
            int enduserListId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var stream = await PermissionService.GetPermissionsForEnduserListAsync(id, enduserListId);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"EUList{enduserListId}.xlsx");
        }

        /// <summary>
        /// Sets enduser permissions for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="permissions">
        /// A list of permissions to set. Each element must contain enduser Id and corresponded permission.
        /// Allowable permission values are None and View. The list of permissions should not contain duplicate endusers.
        /// </param>
        /// <remarks>
        /// The service does not remove corresponded 'whole' enduser list permissions. Use endpoint
        /// DELETE documents/{id}/enduserlistpermissions/{enduserListId} for this.
        /// </remarks>
        /// 
        /// <response code="200">OK</response>
        /// <response code="400">Validation error (see error message)</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or the specified enduser not found</response>
        [Route("{id:long}/enduserpermissions", Name = "SetEnduserPermissions")]
        [HttpPatch]
        public async Task<IActionResult> SetEnduserPermissionsAsync(
            long id,
            [FromBody] IList<PermissionDto> permissions)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var endusers = await PermissionService.ValidatePermissionsAsync(permissions);

            if (endusers.Any())
                await PermissionService.SetEnduserPermissionsAsync(id, permissions, endusers);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }

        /// <summary>
        /// Uploads enduser permissions from Excel for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <remarks>
        /// The service always returns OK if it can open Excel document from the request stream.
        /// See number of permissions changed and errors in response object.
        /// Input Excel should contain columns Id, Name, Full Name, Permission.
        /// The service uses values Id and Permission starting from the second row.
        /// If uploading succeeded, the service removes all corresponded 'whole' enduser list permissions if any.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Request doesn't contain valid Excel document</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/enduserpermissions/upload", Name = "UploadDocumentEnduserPermissions")]
        [HttpPatch]
        [Produces(typeof(ExcelUploadDto))]
        public async Task<IActionResult> UploadDocumentEnduserPermissionsAsync(
            long id)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var result = await PermissionService.SetEnduserPermissionsAsync(id, Request);
            if (result.UpdatedRecordsCount > 0)
                await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                    id, PermissionService.GetTimestamp());

            return Ok(result);
        }

        /// <summary>
        /// Deletes all individual enduser permissions for the specified enduser list
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserListId">Id of the enduser list</param>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or permission not found</response>
        [Route("{id:long}/enduserpermissions", Name = "DeleteIndividualEnduserPermissions")]
        [HttpDelete]
        public async Task<IActionResult> DeleteIndividualEnduserPermissionsAsync(
            long id,
            [FromQuery] int enduserListId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (await PermissionService.DeleteIndividualEnduserPermissionsAsync(id, enduserListId) == 0)
                return NotFound($"No individual permissions for enduser list {enduserListId}");

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }

        /// <summary>
        /// Deletes permission of the specified enduser
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="enduserId">Id of the enduser to delete</param>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or enduser with the specified Id not found</response>
        [Route("{id:long}/enduserpermissions/{enduserId:int}", Name = "DeleteEnduserPermission")]
        [HttpDelete]
        public async Task<IActionResult> DeleteEnduserPermissionAsync(
            long id,
            int enduserId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await PermissionService.DeleteEnduserPermissionAsync(id, enduserId))
                return Forbid();

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }
    }
}
