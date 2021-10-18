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
    /// Controller for handling document aliases
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class AliasesController : ControllerBase
    {
        public AliasesController(AliasService service)
        {
            AliasService = service;
        }

        private AliasService AliasService { get; }

        /// <summary>
        /// Gets document alias
        /// </summary>
        /// <param name="aliasId">Id of the alias</param>
        /// <returns>Alias</returns>
        /// <response code="200">Document found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Alias not found</response>
        [Route("aliases/{aliasId:long}", Name = "GetAlias")]
        [HttpGet]
        [Produces(typeof(AliasDto))]
        public async Task<IActionResult> GetAliasAsync(
            long aliasId)
        {
            var alias = await AliasService.GetAliasByIdAsync(aliasId);
            if (alias == null)
                return NotFound();

            if (!await AliasService.HasPermissionAsync(
                alias.DocumentId, Permission.View, ResourceStatus.Exists))
                return Forbid();

            var result = AliasService.AliasToDto(alias);
            result.FillLinks(Url);

            return Ok(result);
        }

        /// <summary>
        /// Creates document alias
        /// </summary>
        /// <param name="alias">An object containing alias properties</param>
        /// <returns>Created alias</returns>
        /// <response code="201">Alias created</response>
        /// <response code="400">The specified alias is wrong</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("aliases", Name = "CreateAlias")]
        [HttpPost]
        [Produces(typeof(AliasDto))]
        public async Task<IActionResult> CreateAliasAsync(
            [FromBody] AliasToCreateDto alias)
        {
            var documentId = alias?.DocumentId ?? 0;
            if (!await AliasService.HasPermissionAsync(
                documentId, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            alias = await AliasService.ValidateAliasAsync(alias);

            var result = await AliasService.CreateAliasAsync(alias);
            result.FillLinks(Url);
            var uri = Url.RelativeLink("GetAlias", new { aliasId = result.Id });

            return Created(uri, result);
        }

        /// <summary>
        /// Deletes document alias
        /// </summary>
        /// <param name="aliasId">Id of the alias</param>
        /// <response code="200">Alias has been successfully deleted</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Alias not found</response>
        [Route("aliases/{aliasId:long}", Name = "DeleteAlias")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAliasAsync(
            long aliasId)
        {
            var alias = await AliasService.GetAliasByIdAsync(aliasId);
            if (alias == null || !await AliasService.HasPermissionAsync(
                    alias.DocumentId, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            if (!await AliasService.DeleteAliasAsync(aliasId))
                return Forbid();

            return Ok();
        }

        /// <summary>
        /// Changes link to document
        /// </summary>
        /// <param name="aliasId">Id of the alias</param>
        /// <param name="alias">An object with link to new document</param>
        /// <returns>Updated alias</returns>
        /// <response code="200">Alias has been successfully updated</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Alias or new document not found</response>
        [Route("aliases/{aliasId:long}", Name = "UpdateAlias")]
        [HttpPatch]
        [Produces(typeof(AliasDto))]
        public async Task<IActionResult> UpdateAliasAsync(
            long aliasId,
            [FromBody] AliasPatchDto alias)
        {
            var documentAlias = await AliasService.GetAliasByIdAsync(aliasId);
            if (documentAlias == null)
                return Forbid();

            var oldDocumentId = documentAlias.DocumentId;
            var newDocumentId = alias.DocumentId;

            if (!await AliasService.HasPermissionAsync(
                    oldDocumentId, Permission.Manage, ResourceStatus.Exists) ||
                !await AliasService.HasPermissionAsync(
                    newDocumentId, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var result = await AliasService.UpdateAliasLinkAsync(documentAlias, newDocumentId);
            result.FillLinks(Url);

            return Ok(result);
        }

        /// <summary>
        /// Gets aliases
        /// </summary>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <returns>Requested page</returns>
        /// <remarks>
        /// Example of query string: ?$top=20&#38;$skip=0&#38;$orderby=Namespace asc&#38;$filter=Namespace eq 'MyNamespace'.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Wrong parameters</response>
        /// <response code="401">User is not authenticated</response>
        [Route("aliases", Name = "GetAliases")]
        [HttpGet]
        [Produces(typeof(PageDto<AliasDto>))]
        public async Task<IActionResult> GetAliasesAsync(
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            var aliases = await AliasService.GetAliasesAsync();
            var opts = ODataBuilder.BuildOptions<AliasDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(aliases, opts);
            foreach (var alias in page.Entities)
            {
                alias.FillLinks(Url);
            }

            return Ok(new PageDto<AliasDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetAliases", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetAliases", opts, page.Next)
                }
            });
        }

        /// <summary>
        /// Gets document by alias
        /// </summary>
        /// <param name="namespace">An alias namespace</param>
        /// <param name="alias">A document alias</param>
        /// <returns>Document</returns>
        /// <response code="200">Document found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Alias not found</response>
        [Route("alias/{namespace:length(1,50)}/{alias:length(1,80)}", Name = "GetDocumentByAlias")]
        [HttpGet]
        [Produces(typeof(DocumentDto))]
        public async Task<IActionResult> GetDocumentByAliasAsync(
            string @namespace,
            string alias)
        {
            if (await AliasService.IsInRoleAsync(Role.Enduser))
                return Forbid();

            var document = await AliasService.GetDocumentByAliasAsync(@namespace, alias);
            if (document == null)
                return NotFound();

            if (!await AliasService.HasPermissionAsync(
                document.Id, Permission.View, ResourceStatus.Exists))
                return Forbid();

            var result = await AliasService.DocumentToDtoAsync(document);
            await result.FillLinksAsync(AliasService, Url);

            await AliasService.CreateOrUpdateAccessedDocumentTimestampAsync(document.Id,
                AliasService.GetTimestamp());

            return Ok(result);
        }

        /// <summary>
        /// Gets published revision of the document by alias
        /// </summary>
        /// <param name="namespace">An alias namespace</param>
        /// <param name="alias">A document alias</param>
        /// <returns>Document</returns>
        /// <response code="200">Revision found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Alias not found</response>
        [Route("alias/{namespace:length(1,50)}/{alias:length(1,80)}/revisions/published", Name = "GetPublishedRevisionByAlias")]
        [HttpGet]
        [Produces(typeof(RevisionDto))]
        public async Task<IActionResult> GetPublishedRevisionByAliasAsync(
            string @namespace,
            string alias)
        {
            var document = await AliasService.GetBareDocumentByAliasAsync(@namespace, alias);
            if (document == null)
                return NotFound();

            if (!await AliasService.HasPermissionAsync(
                document.Id, Permission.View, ResourceStatus.Exists))
                return Forbid();

            if (document.PublishedRevisionId == null)
                return NotFound();

            var revision = await AliasService.GetRevisionByIdAsync(document.PublishedRevisionId.Value);
            if (revision == null)
                return NotFound();

            await AliasService.CreateOrUpdateAccessedDocumentTimestampAsync(
                revision.DocumentId, AliasService.GetTimestamp());

            return Ok(await AliasService.RevisionToDtoAsync(revision));
        }
    }
}