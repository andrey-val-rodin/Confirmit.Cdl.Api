using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public class CdlServiceClient
    {
        public const string Postfix = "_CDL_API_TESTS";
        public const string Prefix = "__CdlApiIntegrationTests__";
        protected IServiceScope Scope;
        public ICdl Service;

        public CdlServiceClient()
        {
        }

        public CdlServiceClient(IServiceScope scope)
        {
            Scope = scope;
            Service = scope.GetService<ICdl>();
        }

        #region root

        public async Task<RootDto> GetRootAsync(
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            var response = await Service.GetRootAsync();
            Assert.True(expectedStatusCode == response.StatusCode,
                $"Wrong status code. Expected: {expectedStatusCode} Actual: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            if (expectedErrorMessage != null)
                Assert.True(content.Contains(expectedErrorMessage),
                    $"Response doesn't contain expected error message '{expectedErrorMessage}'");

            return JsonConvert.DeserializeObject<RootDto>(content);
        }

        #endregion

        #region documents

        public async Task<DocumentDto> PostDocumentAsync(
            DocumentToCreateDto document = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.Created,
            string expectedErrorMessage = null,
            bool validate = true)
        {
            if (validate)
                document = Validate(document);
            return await ResponseHandler.HandleRequestAsync<DocumentDto>(
                await Service.PostDocumentAsync(document),
                expectedStatusCode, expectedErrorMessage);
        }

        private DocumentToCreateDto Validate(DocumentToCreateDto document)
        {
            document ??= new DocumentToCreateDto
            {
                Name = Prefix,
                SourceCode = ""
            };

            if (string.IsNullOrEmpty(document.Name))
                document.Name = Prefix;
            else if (!document.Name.StartsWith(Prefix))
                document.Name = Prefix + document.Name;
            if (document.SourceCode == null)
                document.SourceCode = "";

            return document;
        }

        public async Task<JObject> DeleteDocumentAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<JObject>(
                await Service.DeleteDocumentAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<DocumentDto> PatchDocumentAsync(
            long documentId,
            DocumentPatchDto patch,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<DocumentDto>(
                await Service.PatchDocumentAsync(documentId, patch),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<DocumentDto> GetDocumentAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<DocumentDto>(
                await Service.GetDocumentAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<string> GetDocumentPublicMetadata(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null,
            string accept = "text/plain")
        {
            var response = await Service.GetDocumentPublicMetadataAsync(documentId, accept);
            Assert.True(expectedStatusCode == response.StatusCode,
                $"Wrong status code. Expected: {expectedStatusCode} Actual: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            if (expectedErrorMessage != null)
                Assert.True(content.Contains(expectedErrorMessage),
                    $"Response doesn't contain expected error message '{expectedErrorMessage}'");

            return response.StatusCode == HttpStatusCode.OK ? content : null;
        }

        public async Task<PageDto<DocumentShortDto>> GetDocumentsAsync(
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<DocumentShortDto>>(
                await Service.GetDocumentsAsync(skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<AccessedDocumentDto>> GetAccessedDocumentsAsync(
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<AccessedDocumentDto>>(
                await Service.GetAccessedDocumentsAsync(skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PutPublishedRevisionAsync(
            long documentId,
            RevisionToPublishDto revision,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PutPublishedRevisionAsync(documentId, revision),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<RevisionDto> GetPublishedRevisionAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<RevisionDto>(
                await Service.GetPublishedRevisionAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeletePublishedRevisionAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeletePublishedRevisionAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region revisions

        public async Task<PageDto<RevisionShortDto>> GetPublishedRevisionsAsync(
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<RevisionShortDto>>(
                await Service.GetPublishedRevisionsAsync(skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<AccessedRevisionDto>> GetAccessedRevisionsAsync(
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<AccessedRevisionDto>>(
                await Service.GetAccessedRevisionsAsync(skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<RevisionDto> GetRevisionAsync(
            long revisionId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<RevisionDto>(
                await Service.GetRevisionAsync(revisionId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteRevisionAsync(
            long revisionId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteRevisionAsync(revisionId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<RevisionDto> PostRevisionAsync(
            long documentId,
            RevisionToCreateDto revision = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.Created,
            string expectedErrorMessage = null)
        {
            if (revision == null)
                revision = new RevisionToCreateDto();

            return await ResponseHandler.HandleRequestAsync<RevisionDto>(
                await Service.PostRevisionAsync(documentId, revision),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<RevisionShortDto>> GetRevisionsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<RevisionShortDto>>(
                await Service.GetRevisionsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<string> GetPublicMetadataForPublishedRevisionAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null,
            string accept = "text/plain")
        {
            var response = await Service.GetPublicMetadataForPublishedRevisionAsync(documentId, accept);
            Assert.True(expectedStatusCode == response.StatusCode,
                $"Wrong status code. Expected: {expectedStatusCode} Actual: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            if (expectedErrorMessage != null)
                Assert.True(content.Contains(expectedErrorMessage),
                    $"Response doesn't contain expected error message '{expectedErrorMessage}'");

            return response.StatusCode == HttpStatusCode.OK ? content : null;
        }

        #endregion

        #region archived documents

        public async Task<PageDto<DocumentShortDto>> GetArchivedDocumentsAsync(
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<DocumentShortDto>>(
                await Service.GetArchivedDocumentsAsync(skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<DocumentDto> GetArchivedDocumentAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<DocumentDto>(
                await Service.GetArchivedDocumentAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<DocumentDto> RestoreArchivedDocumentAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.Created,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<DocumentDto>(
                await Service.RestoreArchivedDocumentAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteArchivedDocumentAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteArchivedDocumentAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<ExpirationPeriod> GetExpirationPeriodAsync(
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<ExpirationPeriod>(
                await Service.GetExpirationPeriodAsync(),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region user permissions

        public async Task<PageDto<UserPermissionFullDto>> GetUserPermissionsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<UserPermissionFullDto>>(
                await Service.GetUserPermissionsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<OrganizationDto>> GetUserPermissionsCompaniesAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationDto>>(
                await Service.GetUserPermissionsCompaniesAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PatchUserPermissionsAsync(
            long documentId,
            IList<UserPermissionDto> permissions,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PatchUserPermissionsAsync(documentId, permissions),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PatchUserPermissionsAsync(
            long documentId,
            IList<PermissionStub> permissions,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PatchUserPermissionsAsync(documentId, permissions),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteIndividualUserPermissionsAsync(
            long documentId,
            int companyId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteIndividualUserPermissionsAsync(documentId, companyId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteUserPermissionAsync(
            long documentId,
            int userId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteUserPermissionAsync(documentId, userId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region company permissions

        public async Task<PageDto<OrganizationDto>> GetAllCompaniesAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationDto>>(
                await Service.GetAllCompaniesAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<OrganizationPermissionDto>> GetCompanyPermissionsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationPermissionDto>>(
                await Service.GetCompanyPermissionsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PutCompanyPermissionAsync(
            long documentId,
            PermissionDto permission,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PutCompanyPermissionAsync(documentId, permission),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PutCompanyPermissionAsync(
            long documentId,
            PermissionStub permission,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PutCompanyPermissionAsync(documentId, permission),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteCompanyPermissionAsync(
            long documentId,
            int companyId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteCompanyPermissionAsync(documentId, companyId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region enduser permissions

        public async Task<PageDto<EnduserPermissionFullDto>> GetEnduserPermissionsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<EnduserPermissionFullDto>>(
                await Service.GetEnduserPermissionsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<EnduserPermissionFullDto>> GetEnduserPermissionsForEnduserListAsync(
            long documentId,
            int listId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<EnduserPermissionFullDto>>(
                await Service.GetEnduserPermissionsForEnduserListAsync(documentId, listId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<OrganizationDto>> GetEnduserPermissionsEnduserListsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationDto>>(
                await Service.GetEnduserPermissionsEnduserListsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PatchEnduserPermissionsAsync(
            long documentId,
            IList<PermissionDto> permissions,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PatchEnduserPermissionsAsync(documentId, permissions),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PatchEnduserPermissionsAsync(
            long documentId,
            IList<PermissionStub> permissions,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PatchEnduserPermissionsAsync(documentId, permissions),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteIndividualEnduserListPermissionsAsync(
            long documentId,
            int listId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteIndividualEnduserListPermissionsAsync(documentId, listId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteEnduserPermissionAsync(
            long documentId,
            int enduserId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteEnduserPermissionAsync(documentId, enduserId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region enduser list permissions

        public async Task<PageDto<OrganizationDto>> GetAllEnduserListsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationDto>>(
                await Service.GetAllEnduserListsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<OrganizationPermissionDto>> GetEnduserListPermissionsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationPermissionDto>>(
                await Service.GetEnduserListPermissionsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PutEnduserListPermissionAsync(
            long documentId,
            PermissionDto permission,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PutEnduserListPermissionAsync(documentId, permission),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PutEnduserListPermissionAsync(
            long documentId,
            PermissionStub permission,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PutEnduserListPermissionAsync(documentId, permission),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteEnduserListPermissionAsync(
            long documentId,
            int listId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteEnduserListPermissionAsync(documentId, listId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region Excel

        public async Task<Stream> DownloadEnduserPermissionsAsync(
            long documentId, int enduserListId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            return await ResponseHandler.HandleRequestAsync(
                await Service.DownloadEnduserPermissionsAsync(documentId, enduserListId),
                expectedStatusCode);
        }

        public async Task<ExcelUploadDto> UploadEnduserPermissionsAsync(
            long documentId,
            Stream stream,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            var content = new StreamContent(stream);
            content.Headers.ContentType =
                new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return await ResponseHandler.HandleRequestAsync<ExcelUploadDto>(
                await Service.UploadEnduserPermissionsAsync(documentId, content),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region selected enduser lists

        public async Task<PageDto<OrganizationDto>> GetSelectedEnduserListsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<OrganizationDto>>(
                await Service.GetSelectedEnduserListsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task PutSelectedEnduserListAsync(
            long documentId,
            int permission,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.PutSelectedEnduserListAsync(documentId, permission),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteSelectedEnduserListsAsync(
            long documentId,
            int enduserListId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteSelectedEnduserListsAsync(documentId, enduserListId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region permission

        public async Task<Permission> GetPermissionAsync(
            long documentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<Permission>(
                await Service.GetPermissionAsync(documentId),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region commits

        public async Task<PageDto<CommitDto>> GetCommitsAsync(
            long documentId,
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<CommitDto>>(
                await Service.GetCommitsAsync(documentId, skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion

        #region aliases

        public async Task<AliasDto> GetAliasAsync(
            long aliasId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<AliasDto>(
                await Service.GetAliasAsync(aliasId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<AliasDto> PostAliasAsync(
            AliasToCreateDto alias = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.Created,
            string expectedErrorMessage = null)
        {
            if (alias == null)
                alias = new AliasToCreateDto();

            return await ResponseHandler.HandleRequestAsync<AliasDto>(
                await Service.PostAliasAsync(alias),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task DeleteAliasAsync(
            long aliasId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            await ResponseHandler.HandleRequestAsync(
                await Service.DeleteAliasAsync(aliasId),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<AliasDto> PatchAliasAsync(
            long aliasId,
            AliasPatchDto alias,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<AliasDto>(
                await Service.PatchAliasAsync(aliasId, alias),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<PageDto<AliasDto>> GetAliasesAsync(
            int? skip = null,
            int? top = null,
            string orderBy = null,
            string filter = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<PageDto<AliasDto>>(
                await Service.GetAliasesAsync(skip, top, orderBy, filter),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<DocumentDto> GetDocumentByAliasAsync(
            string @namespace,
            string alias,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<DocumentDto>(
                await Service.GetDocumentByAliasAsync(@namespace, alias),
                expectedStatusCode, expectedErrorMessage);
        }

        public async Task<RevisionDto> GetPublishedRevisionByAliasAsync(
            string @namespace,
            string alias,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedErrorMessage = null)
        {
            return await ResponseHandler.HandleRequestAsync<RevisionDto>(
                await Service.GetPublishedRevisionByAliasAsync(@namespace, alias),
                expectedStatusCode, expectedErrorMessage);
        }

        #endregion
    }
}
