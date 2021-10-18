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
    /// Controller for getting commits
    /// </summary>
    [ApiController]
    [Route("documents")]
    [Authorize("DefaultAccessPolicy")]
    public class CommitsController : ControllerBase
    {
        public CommitsController(CommitService service)
        {
            CommitService = service;
        }

        private CommitService CommitService { get; }

        /// <summary>
        /// Gets all commits
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="top">Top query option</param>
        /// <param name="skip">Skip query option</param>
        /// <param name="orderby">Orderby query option</param>
        /// <param name="filter">Filter query option</param>
        /// <remarks>
        /// The service automatically creates commit for each operation on document or revision.
        /// </remarks>
        /// <returns>Requested page</returns>
        /// <response code="200">OK</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">Access denied</response>
        /// <response code="404">Document not found</response>
        [Route("{id:long}/commits", Name = "GetCommits")]
        [HttpGet]
        [Produces(typeof(PageDto<CommitDto>))]
        public async Task<IActionResult> GetCommitsAsync(
            long id,
            [FromQuery(Name = "$top")] int top,
            [FromQuery(Name = "$skip")] int skip,
            [FromQuery(Name = "$orderby")] string orderby,
            [FromQuery(Name = "$filter")] string filter)
        {
            if (!await CommitService.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
                return Forbid();

            var commits = CommitService.GetQuery(id);
            var opts = ODataBuilder.BuildOptions<CommitDto>(Request);
            var page = await BaseService.ApplyODataQueryOptionsAsync(commits, opts);
            foreach (var commit in page.Entities)
            {
                commit.FillLinks(Url);
            }

            return Ok(new PageDto<CommitDto>
            {
                TotalCount = page.TotalCount,
                Items = page.Entities,
                Links = new PageLinks
                {
                    PreviousPage = BaseService.GetPageLink(Url, "GetCommits", opts, page.Previous),
                    NextPage = BaseService.GetPageLink(Url, "GetCommits", opts, page.Next)
                }
            });
        }
    }
}