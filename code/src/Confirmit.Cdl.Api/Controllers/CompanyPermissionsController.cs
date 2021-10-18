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
    /// Controller for handling user permissions on the specified document
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class CompanyPermissionsController : ControllerBase
    {
        public CompanyPermissionsController(UserPermissionService service)
        {
            PermissionService = service;
        }

        private UserPermissionService PermissionService { get; }

        /// <summary>
        /// Returns all companies whose users have access to the document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <remarks>
        /// Response contains companies from both users and companies permissions.
        /// </remarks>
        /// <returns>List of permissions</returns>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/companies", Name = "GetAllCompanies")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationDto>))]
        public async Task<IActionResult> GetAllCompaniesAsync(
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

            var companies = PermissionService.GetAllOrganizations(id);
            var opts = ODataBuilder.BuildOptions<OrganizationDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(companies, opts);

            return Ok(new PageDto<OrganizationDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetAllCompanies", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetAllCompanies", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Returns list of company permissions for the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// You can use this endpoint to get all companies with the 'whole' permission.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/companypermissions", Name = "GetCompanyPermissions")]
        [HttpGet]
        [Produces(typeof(PageDto<OrganizationPermissionDto>))]
        public async Task<IActionResult> GetCompanyPermissionsAsync(
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
                    PreviousPage = BaseService.GetPageLink(Url, "GetCompanyPermissions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetCompanyPermissions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Sets permission for the whole company
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="permission">contains Id of company and permission "View", "Manage" or "None"</param>
        /// <response code="200">OK</response>
        /// <response code="400">Validation error (see error message)</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or company not found</response>
        [Route("{id:long}/companypermissions", Name = "SetCompanyPermission")]
        [HttpPut]
        public async Task<IActionResult> SetCompanyPermissionAsync(
            long id,
            [FromBody] PermissionDto permission)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            await PermissionService.ValidatePermissionCompanyAsync(permission.Id);

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            await PermissionService.SetCompanyPermissionAsync(id, permission.Id, permission.Permission);
            return Ok();
        }

        /// <summary>
        /// Deletes permission of the whole company
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="companyId">Id of the company to delete</param>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or permission not found</response>
        [Route("{id:long}/companypermissions/{companyId:int}", Name = "DeleteCompanyPermission")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCompanyPermissionAsync(
            long id,
            int companyId)
        {
            if (!await PermissionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await PermissionService.DeleteCompanyPermissionAsync(id, companyId))
                return NotFound($"Company {companyId} not found");

            await PermissionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                id, PermissionService.GetTimestamp());

            return Ok();
        }
    }
}
