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
    /// Controller for handling revision resources
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class RevisionsController : ControllerBase
    {
        public RevisionsController(RevisionService service)
        {
            RevisionService = service;
        }

        private RevisionService RevisionService { get; }

        /// <summary>
        /// Gets all published revisions
        /// </summary>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$orderby=Modified asc&#38;$filter=Type ne 'ProgramDashboard' and (contains(Name, 'Foo') or contains(CompanyName, 'Foo') or contains(CreatedByName, 'Foo') or contains(ModifiedByName, 'Foo')).
        /// There are no restrictions on the number of items in response, so you should specify reasonable parameters $top and $skip,
        /// otherwise service will return all available revisions in accordance with the specified $filter.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        [Route("revisions/published", Name = "GetPublishedRevisions")]
        [HttpGet]
        [Produces(typeof(PageDto<RevisionShortDto>))]
        public async Task<IActionResult> GetPublishedRevisionsAsync(
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            var revisions = await RevisionService.GetPublishedRevisionsAsync();
            var opts = ODataBuilder.BuildOptions<RevisionShortDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(revisions, opts);

            return Ok(new PageDto<RevisionShortDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetPublishedRevisions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetPublishedRevisions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Gets all published revisions accessed by the current user or enduser
        /// </summary>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$orderby=Accessed desc&#38;$filter=Type ne 'ProgramDashboard' and (contains(Name, 'Foo') or contains(CompanyName, 'Foo') or contains(CreatedByName, 'Foo') or contains(ModifiedByName, 'Foo')).
        /// There are no restrictions on the number of items in response, so you should specify reasonable parameters $top and $skip,
        /// otherwise service will return all available revisions in accordance with the specified $filter.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        [Route("revisions/published/accessed", Name = "GetAccessedRevisions")]
        [HttpGet]
        [Produces(typeof(AccessedRevisionDto))]
        public async Task<IActionResult> GetAccessedRevisionsAsync(
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            var revisions = await RevisionService.GetAccessedRevisionsAsync();
            var opts = ODataBuilder.BuildOptions<AccessedRevisionDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(revisions, opts);

            return Ok(new PageDto<AccessedRevisionDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetAccessedRevisions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetAccessedRevisions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Gets the specified revision
        /// </summary>
        /// <param name="revisionId">Id of the revision to get</param>
        /// <response code="200">Revision found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Revision not found</response>
        [Route("revisions/{revisionId:long}", Name = "GetRevisionById")]
        [HttpGet]
        [Produces(typeof(RevisionDto))]
        public async Task<IActionResult> GetRevisionByIdAsync(
            long revisionId)
        {
            var revision = await RevisionService.GetRevisionByIdAsync(revisionId);
            if (revision == null)
                return NotFound();

            if (!await RevisionService.HasPermissionAsync(revision.DocumentId, Permission.View, ResourceStatus.Exists))
                return Forbid();

            await RevisionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                revision.DocumentId, RevisionService.GetTimestamp());

            var result = await RevisionService.RevisionToDtoAsync(revision);
            result.FillLinks(Url);

            return Ok(result);
        }

        /// <summary>
        /// Deletes the specified revision
        /// </summary>
        /// <param name="revisionId">Id of the revision to delete</param>
        /// <response code="200">Revision has been successfully deleted</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Revision not found</response>
        [Route("revisions/{revisionId:long}", Name = "DeleteRevision")]
        [HttpDelete]
        public async Task<IActionResult> DeleteRevisionAsync(
            long revisionId)
        {
            var revision = await RevisionService.GetRevisionByIdAsync(revisionId);
            if (revision == null)
                return NotFound();

            if (!await RevisionService.HasPermissionAsync(revision.DocumentId, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await RevisionService.DeleteRevisionAsync(revision))
                return NotFound();

            await RevisionService.CreateAndAddCommitAsync(revision.DocumentId, null, revision.Number,
                Action.Delete, RevisionService.GetTimestamp());

            await RevisionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                revision.DocumentId, RevisionService.GetTimestamp());

            return Ok();
        }

        /// <summary>
        /// Creates revision of the specified document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="revision">An object with revision properties</param>
        /// <returns>Created revision</returns>
        /// <remarks>
        /// Payload can contain the following properties:
        /// 
        /// • name - a new document name. If not specified, the service takes current value from the document
        /// 
        /// • sourceCode - a string containing CDL. The service doesn't require sourceCode to be equal to the current
        /// sourceCode of the document. If not specified, the service takes current value from the document
        /// 
        /// • action - Determines whether the service should publish new revision.
        /// Possible value is "CreatePublished" or "CreateSnapshot". Default is "CreatePublished"
        /// 
        /// • publicMetadata - a string containing public metadata JSON.
        /// If not specified, the service takes current value from the document
        /// 
        /// • privateMetadata - a string containing private metadata JSON.
        /// If not specified, the service takes current value from the document.
        ///
        /// If action is CreatePublished and the document has hub or survey, the service verifies that current user
        /// has read access to them. This is because when document is published, the user grants
        /// implicit read permissions for hub and survey to all viewers of the document.
        /// </remarks>
        /// <response code="201">Revision created</response>
        /// <response code="400">Validation error of input</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document, hub or survey not found</response>
        [Route("{id:long}/revisions", Name = "CreateRevision")]
        [HttpPost]
        [Produces(typeof(RevisionDto))]
        public async Task<IActionResult> CreateRevisionAsync(
            long id,
            [FromBody] RevisionToCreateDto revision)
        {
            if (!await RevisionService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var document = await RevisionService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            var action = RevisionService.GetAction(revision.Action);
            var result = await RevisionService.CreateRevisionAsync(document, revision, action);
            result.FillLinks(Url);

            await RevisionService.CreateAndAddCommitAsync(document.Id,
                result.Id, result.Number, action, result.Created);

            await RevisionService.CreateOrUpdateAccessedDocumentTimestampAsync(
                result.DocumentId, result.Created);

            var uri = Url.RelativeLink("GetRevisionById", new { revisionId = result.Id });
            return Created(uri, result);
        }

        /// <summary>
        /// Gets all revisions of the document
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$orderby=Created asc&#38;$filter=Type eq 'ReportingDashboard'.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/revisions", Name = "GetDocumentRevisions")]
        [HttpGet]
        [Produces(typeof(PageDto<RevisionShortDto>))]
        public async Task<IActionResult> GetDocumentRevisionsAsync(
            long id,
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (await RevisionService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            if (await RevisionService.GetDocumentByIdAsync(id) == null)
                return NotFound();

            if (!await RevisionService.HasPermissionAsync(id, Permission.View, ResourceStatus.Exists))
                return Forbid();

            var revisions = await RevisionService.GetDocumentRevisionsAsync(id);
            var opts = ODataBuilder.BuildOptions<RevisionShortDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(revisions, opts);

            return Ok(new PageDto<RevisionShortDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetDocumentRevisions", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetDocumentRevisions", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Gets public metadata of the published revision
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>Public metadata for published revision of the specified document.
        /// This endpoint does not require authentication</returns>
        /// <response code="200">Public metadata found</response>
        /// <response code="404">Document or public metadata not found</response>
        [Route("{id:long}/revisions/published/public-metadata", Name = "GetPublicMetadataForPublishedRevision")]
        [HttpGet]
        [AllowAnonymous]
        [Produces(typeof(string))]
        public async Task<IActionResult> GetPublicMetadataForPublishedRevisionAsync(
            long id)
        {
            var document = await RevisionService.GetBareDocumentByIdAsync(id);
            if (document?.PublishedRevisionId == null)
                return NotFound();

            var result = await RevisionService.GetPublicMetadataAsync(document.PublishedRevisionId.Value);
            if (string.IsNullOrEmpty(result))
                // Do not return NotFound(). Instead, avoid negotiation and throw exception,
                // otherwise response will be 406 Not Acceptable when Accept header is not "application/json"
                throw new NotFoundException();

            return Ok(result);
        }
    }
}
