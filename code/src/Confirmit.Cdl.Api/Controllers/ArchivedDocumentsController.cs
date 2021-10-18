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
    /// Controller for handling document resources
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class ArchivedDocumentsController : ControllerBase
    {
        public ArchivedDocumentsController(DocumentService service)
        {
            DocumentService = service;
        }

        private DocumentService DocumentService { get; }

        /// <summary>
        /// Gets all archived documents
        /// </summary>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// See OData specification to set correct query parameters.
        /// The following OData parameters can be used: $skip, $top, $orderby and $filter.
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$orderby=Created asc&#38;$filter=Type ne 'ProgramDashboard' and (contains(Name, 'Foo') or contains(CompanyName, 'Foo') or contains(CreatedByName, 'Foo') or contains(ModifiedByName, 'Foo')).
        /// There are no restrictions on the number of items in response, so you should specify reasonable parameters $top and $skip,
        /// otherwise service will return all available documents in accordance with the specified $filter.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        [Route("deleted", Name = "GetDeletedDocuments")]
        [HttpGet]
        [Produces(typeof(PageDto<DocumentShortDto>))]
        public async Task<IActionResult> GetDeletedDocumentsAsync(
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (await DocumentService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            var documents = await DocumentService.GetDeletedDocumentsAsync();
            var opts = ODataBuilder.BuildOptions<DocumentShortDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(documents, opts);
            var previousPage = BaseService.GetPageLink(Url, "GetDeletedDocuments", opts, page.Previous);
            var nextPage = BaseService.GetPageLink(Url, "GetDeletedDocuments", opts, page.Next);

            return Ok(new PageDto<DocumentShortDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = previousPage,
                    NextPage = nextPage
                }
            });
        }

        /// <summary>
        /// Gets expiration period for documents
        /// </summary>
        /// <returns>Expiration period in days</returns>
        /// <remarks>
        /// Expiration period is the number of days after which document will be physically deleted.
        /// </remarks>
        /// <response code="200">OK</response>
        [Route("deleted/expiration-period", Name = "GetExpirationPeriod")]
        [HttpGet]
        [Produces(typeof(ExpirationPeriod))]
        public IActionResult GetExpirationPeriod()
        {
            return Ok(DocumentService.GetExpirationPeriod());
        }

        /// <summary>
        /// Gets the specified archived document
        /// </summary>
        /// <param name="id">Id of the document to get</param>
        /// <returns>Document</returns>
        /// <response code="200">Document found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/deleted", Name = "GetArchivedDocumentById")]
        [HttpGet]
        [Produces(typeof(DocumentDto))]
        public async Task<IActionResult> GetArchivedDocumentByIdAsync(
            long id)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Archived))
                return Forbid();

            var result = await DocumentService.GetDeletedDocumentDtoByIdAsync(id);
            if (result == null)
                return Forbid();

            return Ok(result);
        }

        /// <summary>
        /// Restores archived document
        /// </summary>
        /// <param name="id">Id of the document to restore</param>
        /// <response code="200">Document has been successfully restored</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/deleted", Name = "RestoreDocument")]
        [HttpPost]
        [Produces(typeof(DocumentDto))]
        public async Task<IActionResult> RestoreDocumentAsync(
            long id)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Archived))
                return Forbid();

            var restoredDocument = await DocumentService.RestoreDocumentAsync(id);
            if (restoredDocument == null)
                return Forbid();

            await restoredDocument.FillLinksAsync(DocumentService, Url);

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(id, restoredDocument.Modified);

            var uri = Url.RelativeLink("GetDocumentById", new { id = restoredDocument.Id });
            return Created(uri, restoredDocument);
        }

        /// <summary>
        /// Permanently deletes the specified document
        /// </summary>
        /// <param name="id">Id of the document to delete</param>
        /// <response code="200">Document has been successfully deleted</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/deleted", Name = "PhysicallyDeleteDocument")]
        [HttpDelete]
        public async Task<IActionResult> PhysicallyDeleteDocumentAsync(
            long id)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Archived))
                return Forbid();

            if (!await DocumentService.PhysicallyDeleteDocumentAsync(id))
                return NotFound();

            return Ok();
        }
    }
}