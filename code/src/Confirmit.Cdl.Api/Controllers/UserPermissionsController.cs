using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.Tools;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Controllers
{
    /// <summary>
    /// Controller for handling user permissions on the specified document
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class UserPermissionsController : ControllerBase
    {
        public UserPermissionsController(UserPermissionService service)
        {
            PermissionService = service;
        }

        private UserPermissionService PermissionService { get; }

        /// <summary>
        /// Returns user permissions for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// The following OData parameters can be used: $skip, $top, $orderby and $filter.
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$filter=CompanyId eq 1.
        /// </remarks>
        /// <returns>Requested page</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/userpermissions", Name = "GetUserPermissions")]
        [HttpGet]
        [Produces(typeof(PageDto<UserPermissionFullDto>))]
        public async Task<IActionResult> GetUserPermissionsAsync(
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
            var opts = ODataBuilder.BuildOptions<UserPermissionFullDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(permissions, opts);

            return Ok(new PageDto<UserPermissionFullDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetUserPermissions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetUserPermissions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Returns all companies of users that have individual access to the specified document
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
        [Route("{id:long}/userpermissions/companies", Name = "GetUserPermissionsCompanies")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationDto>))]
        public async Task<IActionResult> GetUserPermissionsCompaniesAsync(
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

            var companies = PermissionService.GetUserPermissionsOrganizations(id);
            var opts = ODataBuilder.BuildOptions<OrganizationDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(companies, opts);

            return Ok(new PageDto<OrganizationDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetUserPermissionsCompanies", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetUserPermissionsCompanies", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Sets user permissions for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="permissions">
        /// A list of permissions to set. Each element must contain user Id or user key and corresponded permission.
        /// The list of permissions should not contain duplicate users.
        /// If Id is specified, the service tries to get user info by Id with the help of REST service Users.
        /// Otherwise, the service attempts to get user info by user key.
        /// If user is not found, the service returns 404 Not Found.
        /// If neither Id nor UserKey is specified, the service returns 400 Bad Request
        /// </param>
        /// <response code="200">OK</response>
        /// <response code="400">Validation error (see error message)</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or the specified user not found</response>
        [Route("{id:long}/userpermissions", Name = "SetUserPermissions")]
        [HttpPatch]
        public async Task<IActionResult> SetUserPermissionsAsync(
            long id,
            [FromBody] IList<UserPermissionDto> permissions)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.ValidatePermissionsAsync(permissions);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            await PermissionService.SetUserPermissionsAsync(id, permissions);
            return Ok();
        }

        /// <summary>
        /// Deletes all individual user permissions for the specified company
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="companyId">Id of the company</param>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or user with the specified Id not found</response>
        [Route("{id:long}/userpermissions", Name = "DeleteIndividualUserPermissions")]
        [HttpDelete]
        public async Task<IActionResult> DeleteIndividualUserPermissionsAsync(
            long id,
            int companyId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (await PermissionService.DeleteIndividualUserPermissionsAsync(id, companyId) == 0)
                return NotFound($"No individual permissions for company {companyId}");

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }

        /// <summary>
        /// Deletes permission of the specified user
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="userId">Id of the user to delete</param>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or user with the specified Id not found</response>
        [Route("{id:long}/userpermissions/{userId:int}", Name = "DeleteUserPermission")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUserPermissionAsync(
            long id,
            int userId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await PermissionService.DeleteUserPermissionAsync(id, userId))
                return NotFound();

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }
    }
}
