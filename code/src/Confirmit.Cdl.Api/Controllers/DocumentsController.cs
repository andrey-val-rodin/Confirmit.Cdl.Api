using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Middleware;
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
    public class DocumentsController : ControllerBase
    {
        public DocumentsController(DocumentService service)
        {
            DocumentService = service;
        }

        private DocumentService DocumentService { get; }

        /// <summary>
        /// Gets all documents
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
        [Route("", Name = "GetDocuments")]
        [HttpGet]
        [Produces(typeof(PageDto<DocumentShortDto>))]
        public async Task<IActionResult> GetDocumentsAsync(
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (await DocumentService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            var documents = await DocumentService.GetDocumentsAsync();
            var opts = ODataBuilder.BuildOptions<DocumentShortDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(documents, opts);

            return Ok(new PageDto<DocumentShortDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetDocuments", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetDocuments", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Gets all documents accessed by the current user
        /// </summary>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Documents in response have Access field containing access time of the current user to the document.
        /// If the user has never accessed document, corresponding value will be null.
        /// 
        /// The following OData parameters can be used: $skip, $top, $orderby and $filter.
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$orderby=Accessed desc, Created desc&#38;$filter=Type ne 'ProgramDashboard' and (contains(Name, 'Foo') or contains(CompanyName, 'Foo') or contains(CreatedByName, 'Foo') or contains(ModifiedByName, 'Foo')).
        /// There are no restrictions on the number of items in response, so you should specify reasonable parameters $top and $skip,
        /// otherwise service will return all available documents in accordance with the specified $filter.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        [Route("accessed", Name = "GetAccessedDocuments")]
        [HttpGet]
        [Produces(typeof(PageDto<AccessedDocumentDto>))]
        public async Task<IActionResult> GetAccessedDocumentsAsync(
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (await DocumentService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            var documents = await DocumentService.GetAccessedDocumentsAsync();
            var opts = ODataBuilder.BuildOptions<AccessedDocumentDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(documents, opts);

            return Ok(new PageDto<AccessedDocumentDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetAccessedDocuments", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetAccessedDocuments", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Gets the specified document
        /// </summary>
        /// <param name="id">Id of the document to get</param>
        /// <returns>Document</returns>
        /// <response code="200">Document found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}", Name = "GetDocumentById")]
        [HttpGet]
        [Produces(typeof(DocumentDto))]
        public async Task<IActionResult> GetDocumentByIdAsync(
            long id)
        {
            if (await DocumentService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            if (!await DocumentService.HasPermissionAsync(id, Permission.View, ResourceStatus.Exists))
                return Forbid();

            var result = await DocumentService.GetDocumentDtoByIdAsync(id);
            if (result == null)
                return Forbid();

            await result.FillLinksAsync(DocumentService, Url);

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(id, DocumentService.GetTimestamp());

            return Ok(result);
        }

        /// <summary>
        /// Gets public metadata of the document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>Public metadata for the specified document. This endpoint does not require authentication</returns>
        /// <response code="200">Public metadata found</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or public metadata not found</response>
        [Route("{id:long}/public-metadata", Name = "GetPublicMetadataForDocument")]
        [HttpGet]
        [AllowAnonymous]
        [Produces(typeof(string))]
        public async Task<IActionResult> GetPublicMetadataForDocumentAsync(
            long id)
        {
            var result = await DocumentService.GetPublicMetadataAsync(id);
            if (string.IsNullOrEmpty(result))
                // Do not return NotFound(). Instead, avoid negotiation and throw exception,
                // otherwise response will be 406 Not Acceptable when Accept header is not "application/json"
                throw new NotFoundException();

            return Ok(result);
        }

        /// <summary>
        /// Creates document
        /// </summary>
        /// <param name="document">An object containing document properties</param>
        /// <returns>Created document</returns>
        /// <remarks>
        /// Payload can contain the following properties:
        /// 
        /// • name - a new document name. Required
        /// 
        /// • type - a document type. If no document type is specified, then new document will have type ReportingDashboard
        /// 
        /// • companyId - a company Id. If not specified, the service uses company Id of the current user.
        /// If the specified company is not company of the user, then user must have read access to this company
        /// 
        /// • sourceCode - a string containing CDL. Required
        /// 
        /// • hubId - hub Id. The user must have read access to the specified hub. Optional
        /// 
        /// • linkedSurveyId - survey Id in form 'p0000123'. The user must have read access to the specified survey. Optional
        /// 
        /// • publicMetadata - a string containing public metadata. Optional
        /// 
        /// • privateMetadata - a string containing private metadata. Optional
        ///
        /// • originDocumentId - may contain reference to other document
        /// </remarks>
        /// <response code="201">Document created</response>
        /// <response code="400">Validation error of input</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">User cannot create documents</response>
        /// <response code="404">User has not access to the hub or survey</response>
        [Route("", Name = "CreateDocument")]
        [HttpPost]
        [SkipRefreshing(Order = int.MinValue)] // Skip refreshing current user in background
        [Produces(typeof(DocumentDto))]
        public async Task<IActionResult> CreateDocumentAsync(
            [FromBody] DocumentToCreateDto document)
        {
            if (await DocumentService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            // Current principal must exist in Users table, otherwise DB trigger TR_Document_Insert will fail
            // due to foreign constraint. RefreshUserFilter appends the user in parallel thread,
            // so there is no guarantee that user exists when we insert new document and DB trigger starts working.
            // That's why this action marked with SkipRefreshing attribute.
            // We must refresh current user explicitly
            await DocumentService.DbRefresher.RefreshAsync(); // Inserts new user to DB if necessary

            var result = await DocumentService.CreateDocumentAsync(document);

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(
                result.Id, result.Created);

            var uri = Url.RelativeLink("GetDocumentById", new { id = result.Id });
            return Created(uri, result);
        }

        /// <summary>
        /// Updates the specified document
        /// </summary>
        /// <param name="id">Id of the document to update</param>
        /// <param name="patch">Document fields to update</param>
        /// <returns>Updated document</returns>
        /// <remarks>
        /// Payload can contain the following properties:
        /// 
        /// • name - a new document name
        /// 
        /// • type - a document type
        /// 
        /// • sourceCode - a string containing CDL
        /// 
        /// • companyId - a new document company. If this field is specified, the service will try to change company
        /// of the document. Current user must have permission Read to the new company. If the specified companyId is
        /// wrong or current user has not permission to new company, the service returns 404 Not Found
        /// 
        /// • sourceCodeEditOps - weird parameter to store editor state. Should be removed in the future
        /// 
        /// • hubId - hub Id. The user must have read access to the specified hub
        /// 
        /// • linkedSurveyId - survey Id in form 'p0000123'. The user must have read access to the specified survey
        /// 
        /// • publicMetadata - a string containing public metadata
        /// 
        /// • privateMetadata - a string containing private metadata
        ///
        /// • originDocumentId - a reference to other document
        /// </remarks>
        /// <response code="200">Document has been successfully updated</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found or user has not access to the hub or survey</response>
        [Route("{id:long}", Name = "UpdateDocument")]
        [HttpPatch]
        [Produces(typeof(DocumentDto))]
        public async Task<IActionResult> UpdateDocumentAsync(
            long id,
            [FromBody] DocumentPatchDto patch)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var updatedDocument = await DocumentService.UpdateDocumentAsync(id, patch);
            await updatedDocument.FillLinksAsync(DocumentService, Url);

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(id, updatedDocument.Modified);

            return Ok(updatedDocument);
        }

        /// <summary>
        /// Publishes the specified revision
        /// </summary>
        /// <remarks>
        /// If the document has hub or survey, the service verifies that current user
        /// has read access to them. This is because when document is published, the user grants
        /// implicit read permissions for hub and survey to all viewers of the document.
        /// </remarks>
        /// <param name="id">Id of the document</param>
        /// <param name="revision">Id of revision to publish</param>
        /// <response code="200">Document revision was published</response>
        /// <response code="400">Revision is not specified</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document or revision not found</response>
        [Route("{id:long}/revisions/published", Name = "PublishDocument")]
        [HttpPut]
        public async Task<IActionResult> PublishDocumentAsync(
            long id,
            [FromBody] RevisionToPublishDto revision)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var document = await DocumentService.GetBareDocumentByIdAsync(id);
            if (document == null)
                return Forbid();

            await DocumentService.PublishDocumentAsync(document, revision);

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(id, document.Modified);

            return Ok();
        }

        /// <summary>
        /// Gets published revision of the document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <response code="200">Revision found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found or not published</response>
        [Route("{id:long}/revisions/published", Name = "GetPublishedRevision")]
        [HttpGet]
        [Produces(typeof(RevisionDto))]
        public async Task<IActionResult> GetPublishedRevisionAsync(
            long id)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.View, ResourceStatus.Exists))
                return Forbid();

            var document = await DocumentService.GetBareDocumentByIdAsync(id);
            if (document?.PublishedRevisionId == null)
                return NotFound();

            var revision = await DocumentService.GetRevisionByIdAsync(document.PublishedRevisionId.Value);
            if (revision == null)
                return NotFound();

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(
                revision.DocumentId, DocumentService.GetTimestamp());

            var result = await DocumentService.RevisionToDtoAsync(document, revision);
            result.FillLinks(Url);

            return Ok(result);
        }

        /// <summary>
        /// Unpublishes document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <remarks>
        /// The service does not actually delete published revision. It only resets PublishedRevisionId document property
        /// </remarks>
        /// <response code="200">Document was unpublished</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found or not published</response>
        [Route("{id:long}/revisions/published", Name = "UnpublishDocument")]
        [HttpDelete]
        public async Task<IActionResult> UnpublishDocumentAsync(
            long id)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var document = await DocumentService.GetBareDocumentByIdAsync(id);
            if (document == null)
                return Forbid();

            await DocumentService.UnpublishDocumentAsync(document);

            await DocumentService.CreateOrUpdateAccessedDocumentTimestampAsync(id, document.Modified);

            return Ok();
        }

        /// <summary>
        /// Deletes the specified document
        /// </summary>
        /// <param name="id">Id of the document to delete</param>
        /// <response code="200">Document has been successfully deleted</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}", Name = "DeleteDocument")]
        [HttpDelete]
        public async Task<IActionResult> DeleteDocumentAsync(
            long id)
        {
            if (!await DocumentService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await DocumentService.DeleteDocumentAsync(id))
                return Forbid();

            return Ok(DocumentService.GetArchiveLink(Url, id));
        }
    }
}