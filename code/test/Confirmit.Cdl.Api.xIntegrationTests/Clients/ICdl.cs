using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Clients
{
    public interface ICdl
    {
        #region root

        [Get("")]
        Task<HttpResponseMessage> GetRootAsync();

        #endregion

        #region documents

        [Post("/documents")]
        Task<HttpResponseMessage> PostDocumentAsync([Body] DocumentToCreateDto document);

        [Delete("/documents/{documentId}")]
        Task<HttpResponseMessage> DeleteDocumentAsync(long documentId);

        [Get("/documents/{documentId}")]
        Task<HttpResponseMessage> GetDocumentAsync(long documentId);

        [Get("/documents/{documentId}/public-metadata")]
        Task<HttpResponseMessage> GetDocumentPublicMetadataAsync(long documentId,
            [Header("Accept")] string accept = "text/plain");

        [Patch("/documents/{documentId}")]
        Task<HttpResponseMessage> PatchDocumentAsync(long documentId, [Body] DocumentPatchDto document);

        [Get("/documents")]
        Task<HttpResponseMessage> GetDocumentsAsync(
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/accessed")]
        Task<HttpResponseMessage> GetAccessedDocumentsAsync(
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Put("/documents/{documentId}/revisions/published")]
        Task<HttpResponseMessage> PutPublishedRevisionAsync(long documentId, [Body] RevisionToPublishDto revision);

        [Get("/documents/{documentId}/revisions/published")]
        Task<HttpResponseMessage> GetPublishedRevisionAsync(long documentId);

        [Delete("/documents/{documentId}/revisions/published")]
        Task<HttpResponseMessage> DeletePublishedRevisionAsync(long documentId);

        #endregion

        #region revisions

        [Get("/documents/revisions/published")]
        Task<HttpResponseMessage> GetPublishedRevisionsAsync(
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/revisions/published/accessed")]
        Task<HttpResponseMessage> GetAccessedRevisionsAsync(
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/revisions/{revisionId}")]
        Task<HttpResponseMessage> GetRevisionAsync(long revisionId);

        [Delete("/documents/revisions/{revisionId}")]
        Task<HttpResponseMessage> DeleteRevisionAsync(long revisionId);

        [Post("/documents/{documentId}/revisions")]
        Task<HttpResponseMessage> PostRevisionAsync(long documentId, [Body] RevisionToCreateDto revision);

        [Get("/documents/{documentId}/revisions")]
        Task<HttpResponseMessage> GetRevisionsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/revisions/published/public-metadata")]
        Task<HttpResponseMessage> GetPublicMetadataForPublishedRevisionAsync(long documentId,
            [Header("Accept")] string accept = "text/plain");

        #endregion

        #region archived documents

        [Get("/documents/deleted")]
        Task<HttpResponseMessage> GetArchivedDocumentsAsync(
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/deleted")]
        Task<HttpResponseMessage> GetArchivedDocumentAsync(long documentId);

        [Post("/documents/{documentId}/deleted")]
        Task<HttpResponseMessage> RestoreArchivedDocumentAsync(long documentId);

        [Delete("/documents/{documentId}/deleted")]
        Task<HttpResponseMessage> DeleteArchivedDocumentAsync(long documentId);

        [Get("/documents/deleted/expiration-period")]
        Task<HttpResponseMessage> GetExpirationPeriodAsync();

        #endregion

        #region user permissions

        [Get("/documents/{documentId}/userpermissions")]
        Task<HttpResponseMessage> GetUserPermissionsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/userpermissions/companies")]
        Task<HttpResponseMessage> GetUserPermissionsCompaniesAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Patch("/documents/{documentId}/userpermissions")]
        Task<HttpResponseMessage> PatchUserPermissionsAsync(long documentId, [Body] IList<UserPermissionDto> permissions);

        [Patch("/documents/{documentId}/userpermissions")]
        Task<HttpResponseMessage> PatchUserPermissionsAsync(long documentId, [Body] IList<PermissionStub> permissions);

        [Delete("/documents/{documentId}/userpermissions")]
        Task<HttpResponseMessage> DeleteIndividualUserPermissionsAsync(long documentId, int companyId);

        [Delete("/documents/{documentId}/userpermissions/{userId}")]
        Task<HttpResponseMessage> DeleteUserPermissionAsync(long documentId, int userId);

        #endregion

        #region company permissions

        [Get("/documents/{documentId}/companies")]
        Task<HttpResponseMessage> GetAllCompaniesAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/companypermissions")]
        Task<HttpResponseMessage> GetCompanyPermissionsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Put("/documents/{documentId}/companypermissions")]
        Task<HttpResponseMessage> PutCompanyPermissionAsync(
            long documentId, [Body] PermissionDto permission);

        [Put("/documents/{documentId}/companypermissions")]
        Task<HttpResponseMessage> PutCompanyPermissionAsync(
            long documentId, [Body] PermissionStub permission);

        [Delete("/documents/{documentId}/companypermissions/{companyId}")]
        Task<HttpResponseMessage> DeleteCompanyPermissionAsync(long documentId, int companyId);

        #endregion

        #region enduser permissions

        [Get("/documents/{documentId}/enduserpermissions")]
        Task<HttpResponseMessage> GetEnduserPermissionsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/enduserpermissions/enduserlists/{enduserListId}")]
        Task<HttpResponseMessage> GetEnduserPermissionsForEnduserListAsync(
            long documentId,
            int enduserListId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/enduserpermissions/enduserlists")]
        Task<HttpResponseMessage> GetEnduserPermissionsEnduserListsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/enduserpermissions/enduserlists/{enduserListId}/download")]
        Task<HttpResponseMessage> DownloadEnduserPermissionsAsync(long documentId, int enduserListId);

        [Patch("/documents/{documentId}/enduserpermissions/upload")]
        Task<HttpResponseMessage> UploadEnduserPermissionsAsync(long documentId, [Body] StreamContent streamContent);

        [Patch("/documents/{documentId}/enduserpermissions")]
        Task<HttpResponseMessage> PatchEnduserPermissionsAsync(
            long documentId, [Body] IList<PermissionDto> permissions);

        [Patch("/documents/{documentId}/enduserpermissions")]
        Task<HttpResponseMessage> PatchEnduserPermissionsAsync(
            long documentId, [Body] IList<PermissionStub> permissions);

        [Delete("/documents/{documentId}/enduserpermissions")]
        Task<HttpResponseMessage> DeleteIndividualEnduserListPermissionsAsync(long documentId, int enduserListId);

        [Delete("/documents/{documentId}/enduserpermissions/{enduserId}")]
        Task<HttpResponseMessage> DeleteEnduserPermissionAsync(long documentId, int enduserId);

        #endregion

        #region enduser list permissions

        [Get("/documents/{documentId}/enduserlists")]
        Task<HttpResponseMessage> GetAllEnduserListsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/{documentId}/enduserlistpermissions")]
        Task<HttpResponseMessage> GetEnduserListPermissionsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Put("/documents/{documentId}/enduserlistpermissions")]
        Task<HttpResponseMessage> PutEnduserListPermissionAsync(
            long documentId, [Body] PermissionDto permission);

        [Put("/documents/{documentId}/enduserlistpermissions")]
        Task<HttpResponseMessage> PutEnduserListPermissionAsync(
            long documentId, [Body] PermissionStub permission);

        [Delete("/documents/{documentId}/enduserlistpermissions/{enduserListId}")]
        Task<HttpResponseMessage> DeleteEnduserListPermissionAsync(long documentId, int enduserListId);

        #endregion

        #region selected enduser lists

        [Get("/documents/{documentId}/enduserpermissions/enduserlists/selected")]
        Task<HttpResponseMessage> GetSelectedEnduserListsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Put("/documents/{documentId}/enduserpermissions/enduserlists/selected")]
        Task<HttpResponseMessage> PutSelectedEnduserListAsync(long documentId, [Body] int permission);

        [Delete("/documents/{documentId}/enduserpermissions/enduserlists/selected/{enduserListId}")]
        Task<HttpResponseMessage> DeleteSelectedEnduserListsAsync(long documentId, int enduserListId);

        #endregion

        #region permission

        [Get("/documents/{documentId}/permission")]
        Task<HttpResponseMessage> GetPermissionAsync(long documentId);

        #endregion

        #region commits

        [Get("/documents/{documentId}/commits")]
        Task<HttpResponseMessage> GetCommitsAsync(
            long documentId,
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        #endregion

        #region aliases

        [Get("/documents/aliases/{aliasId}")]
        Task<HttpResponseMessage> GetAliasAsync(long aliasId);

        [Post("/documents/aliases")]
        Task<HttpResponseMessage> PostAliasAsync([Body] AliasToCreateDto alias);

        [Delete("/documents/aliases/{aliasId}")]
        Task<HttpResponseMessage> DeleteAliasAsync(long aliasId);

        [Patch("/documents/aliases/{aliasId}")]
        Task<HttpResponseMessage> PatchAliasAsync(long aliasId, [Body] AliasPatchDto alias);

        [Get("/documents/aliases")]
        Task<HttpResponseMessage> GetAliasesAsync(
            [AliasAs("$skip")] int? skip,
            [AliasAs("$top")] int? top,
            [AliasAs("$orderby")] string orderBy,
            [AliasAs("$filter")] string filter);

        [Get("/documents/alias/{namespace}/{alias}")]
        Task<HttpResponseMessage> GetDocumentByAliasAsync(string @namespace, string alias);

        [Get("/documents/alias/{namespace}/{alias}/revisions/published")]
        Task<HttpResponseMessage> GetPublishedRevisionByAliasAsync(string @namespace, string alias);

        #endregion
    }
}