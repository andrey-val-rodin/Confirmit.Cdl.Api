using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class DocumentTests : TestBase
    {
        public DocumentTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        #region PostDocument

        [Fact]
        public async Task PostDocument_WithValidData_Created()
        {
            const string name = "My Name";
            const string sourceCode = "A nice CDL";

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto
            {
                Name = name,
                SourceCode = sourceCode
            });

            Assert.Equal(Prefix + name, document.Name);
            Assert.Equal(sourceCode, document.SourceCode);
        }

        [Fact]
        public async Task PostDocument_WithValidDataAndSpecialChars_Created()
        {
            const string name = "Name æøå火车指事абв";
            const string sourceCode = "cdl æøå火车指事абв";

            await UseNormalUserAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto
            {
                Name = name,
                SourceCode = sourceCode
            });

            Assert.Equal(Prefix + name, document.Name);
            Assert.Equal(sourceCode, document.SourceCode);
        }

        [Fact]
        public async Task PostDocument_WithInvalidCompanyId_NotFound()
        {
            await UseAdminAsync();
            await PostDocumentAsync(new DocumentToCreateDto { CompanyId = int.MaxValue },
                HttpStatusCode.NotFound, $"Company {int.MaxValue} not found");
        }

        [Fact]
        public async Task PostDocument_ProsUserSpecifiesForeignCompany_Created()
        {
            await UseProsUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { CompanyId = Admin.CompanyId });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostDocument_UserSpecifiesOwnCompany_Created()
        {
            await UseNormalUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { CompanyId = NormalUser.CompanyId });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostDocument_UserSpecifiesForeignCompany_NotFound()
        {
            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { CompanyId = Admin.CompanyId },
                HttpStatusCode.NotFound, $"Company {Admin.CompanyId} not found");
        }

        [Fact]
        public async Task PostDocument_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await PostDocumentAsync(null, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostDocument_NameIsNull_BadRequest()
        {
            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = null, SourceCode = "A nice CDL" },
                HttpStatusCode.BadRequest, "Please provide document name.", false);
        }

        [Fact]
        public async Task PostDocument_NameIsEmpty_BadRequest()
        {
            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = "", SourceCode = "A nice CDL" },
                HttpStatusCode.BadRequest, "Please provide document name.", false);
        }

        [Fact]
        public async Task PostDocument_SourceCodeIsNull_BadRequest()
        {
            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = Prefix },
                HttpStatusCode.BadRequest, "Please provide source code.", false);
        }

        [Fact]
        public async Task PostDocument_SourceCodeIsEmpty_Created()
        {
            // Unlike the name, source code may be empty
            await UseNormalUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { Name = Prefix, SourceCode = "" },
                validate: false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostDocument_CorrectCreatedAndModifiedFields()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();

            Assert.Equal(NormalUser.Id, document.CreatedBy);
            Assert.Equal(NormalUser.FullName, document.CreatedByName);
            Assert.Equal(document.Created, document.Modified);
            Assert.Equal(document.CreatedBy, document.ModifiedBy);
            Assert.Equal(document.CreatedByName, document.ModifiedByName);
        }

        [Fact]
        public async Task PostDocument_TypeIsSpecified_DocumentHasTheSpecifiedType()
        {
            await UseNormalUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { Type = DocumentType.ReportingTemplate });

            Assert.Equal(DocumentType.ReportingTemplate, result.Type);
        }

        [Fact]
        public async Task PostDocument_TypeIsNotSpecified_TypeIsReportingDashboard()
        {
            await UseNormalUserAsync();
            var result = await PostDocumentAsync();

            Assert.Equal(DocumentType.ReportingDashboard, result.Type);
        }

        [Fact]
        public async Task PostDocument_PublicMetadataIsSpecified_DocumentHasPublicMetadata()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata });

            Assert.Equal(publicMetadata, result.PublicMetadata);
        }

        [Fact]
        public async Task PostDocument_PrivateMetadataIsSpecified_DocumentHasPublicMetadata()
        {
            const string privateMetadata = "Private metadata";

            await UseNormalUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { PrivateMetadata = privateMetadata });

            Assert.Equal(privateMetadata, result.PrivateMetadata);
        }

        [Fact]
        public async Task PostDocument_UserDoesNotHaveAccessToHub_NotFound()
        {
            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { HubId = Hub1.Id },
                HttpStatusCode.NotFound, $"Hub {Hub1.Id} not found");
        }

        [Fact]
        public async Task PostDocument_HubDoesNotExist_NotFound()
        {
            await UseAdminAsync();
            await PostDocumentAsync(new DocumentToCreateDto { HubId = 0 },
                HttpStatusCode.NotFound, "Hub 0 not found");
        }

        [Fact]
        public async Task PostDocument_Hub_DocumentHasHub()
        {
            await UseAdminAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { HubId = Hub2.Id });

            Assert.Equal(Hub2.Id, result.HubId);
        }

        [Fact]
        public async Task PostDocument_UserDoesNotHaveAccessToSurvey_NotFound()
        {
            await UseNormalUser2Async();
            await PostDocumentAsync(new DocumentToCreateDto { LinkedSurveyId = Survey1.ProjectId },
                HttpStatusCode.NotFound, $"Survey {Survey1.ProjectId} not found");
        }

        [Fact]
        public async Task PostDocument_SurveyDoesNotExist_NotFound()
        {
            await UseAdminAsync();
            await PostDocumentAsync(new DocumentToCreateDto { LinkedSurveyId = "WRONG" },
                HttpStatusCode.NotFound, "Survey WRONG not found");
        }

        [Fact]
        public async Task PostDocument_Survey_DocumentHasSurvey()
        {
            await UseNormalUserAsync(); // NormalUser has access to Survey1
            var result = await PostDocumentAsync(new DocumentToCreateDto { LinkedSurveyId = Survey1.ProjectId });

            Assert.Equal(Survey1.ProjectId, result.LinkedSurveyId);
        }

        [Fact]
        public async Task PostDocument_OriginDocumentId_DocumentHasOriginDocumentId()
        {
            const int originDocumentId = 123;

            await UseNormalUserAsync();
            var result = await PostDocumentAsync(new DocumentToCreateDto { OriginDocumentId = originDocumentId });

            Assert.Equal(originDocumentId, result.OriginDocumentId);
        }

        #endregion

        #region PatchDocument

        [Fact]
        public async Task PatchDocument_NormalUserHasPermissionView_Forbidden()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchUserPermissionsAsync(id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            await PatchDocumentAsync(id, new DocumentPatchDto { Name = "New name" }, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchDocument_ChangeName_DocumentListHasDocumentWithNewName()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { Name = Prefix + "New name" });

            var page = await GetDocumentsAsync(filter: $"Id eq {id}");

            Assert.Equal(1, page.TotalCount);
            Assert.True(1 == page.Items.Count, "Invalid size of page");
            Assert.True(Prefix + "New name" == page.Items[0].Name, "Name has not been updated");
        }

        [Fact]
        public async Task PatchDocument_DocumentHasNewModifiedFields()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            // Grant permission Manage to NormalUser
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            // Update document under NormalUser account
            await UseNormalUserAsync();
            var updatedDocument = await PatchDocumentAsync(document.Id, new DocumentPatchDto());

            Assert.Equal(NormalUser.Id, updatedDocument.ModifiedBy);
            Assert.Equal(NormalUser.FullName, updatedDocument.ModifiedByName);
        }

        [Fact]
        public async Task PatchDocument_CorrectResult()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            var updatedDocument = await PatchDocumentAsync(document.Id, new DocumentPatchDto
            {
                Name = Prefix + "New name",
                SourceCode = "New source code",
                SourceCodeEditOps = "New source code edit ops",
                PublicMetadata = "New public metadata",
                PrivateMetadata = "New private metadata"
            });

            Assert.Equal(Prefix + "New name", updatedDocument.Name);
            Assert.Equal("New source code", updatedDocument.SourceCode);
            Assert.Equal("New source code edit ops", updatedDocument.SourceCodeEditOps);
            Assert.Equal("New public metadata", updatedDocument.PublicMetadata);
            Assert.Equal("New private metadata", updatedDocument.PrivateMetadata);
        }

        [Fact]
        public async Task PatchDocument_GetDocumentByIdReturnsTheSameResult()
        {
            await UseNormalUserAsync();
            var document = await PostDocumentAsync();
            var updatedDocument = await PatchDocumentAsync(document.Id, new DocumentPatchDto
            {
                Name = Prefix + "New name",
                SourceCode = "New source code",
                SourceCodeEditOps = "New source code edit ops",
                PublicMetadata = "New public metadata",
                PrivateMetadata = "New private metadata"
            });

            var obtainedDocument = await GetDocumentAsync(document.Id);
            Assert.True(
                JsonConvert.SerializeObject(obtainedDocument) == JsonConvert.SerializeObject(updatedDocument),
                "updatedDocument must be equal to the document obtained with the help of GET /documents/{id}");
        }

        [Fact]
        public async Task PatchDocument_WithInvalidCompanyId_NotFound()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { CompanyId = int.MaxValue },
                HttpStatusCode.NotFound, $"Company {int.MaxValue} not found");
        }

        [Fact]
        public async Task PatchDocument_ProsUserSpecifiesForeignCompany_Created()
        {
            await UseProsUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { CompanyId = Admin.CompanyId });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PatchDocument_UserSpecifiesOwnCompany_Created()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { CompanyId = NormalUser.CompanyId });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PatchDocument_UserSpecifiesForeignCompany_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { CompanyId = Admin.CompanyId },
                HttpStatusCode.NotFound, $"Company {Admin.CompanyId} not found");
        }

        [Fact]
        public async Task PatchDocument_Enduser_Forbidden()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchEnduserPermissionsAsync(id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await UseEnduserAsync();
            await PatchDocumentAsync(id, new DocumentPatchDto { Name = "New name" }, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PatchDocument_NameIsNull_OldNameIsUsed()
        {
            const string name = "A name";

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { Name = null, SourceCode = "A nice CDL" });

            Assert.Equal(Prefix + name, result.Name);
        }

        [Fact]
        public async Task PatchDocument_NameIsEmpty_BadRequest()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { Name = "" },
                HttpStatusCode.BadRequest, "Document name cannot be empty.");
        }

        [Fact]
        public async Task PatchDocument_SourceCodeIsNull_OldSourceCodeIsUsed()
        {
            const string sourceCode = "A nice CDL";

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { SourceCode = sourceCode })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { SourceCode = null });

            Assert.Equal(sourceCode, result.SourceCode);
        }

        [Fact]
        public async Task PatchDocument_SourceCodeIsEmpty_SourceCodeIsEmpty()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { SourceCode = "A nice CDL" })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { SourceCode = "" });

            Assert.Equal(string.Empty, result.SourceCode);
        }

        [Fact]
        public async Task PatchDocument_TypeIsNotSpecified_OldTypeIsUsed()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { Type = DocumentType.Automation })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { Type = DocumentType.NotSpecified });

            Assert.Equal(DocumentType.Automation, result.Type);
        }

        [Fact]
        public async Task PatchDocument_ChangeType_DocumentListHasDocumentWithNewType()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { Type = DocumentType.ReportingDashboard })).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { Type = DocumentType.ReportalIntegrationDashboard });

            var page = await GetDocumentsAsync(filter: $"Id eq {id}");

            Assert.Equal(1, page.TotalCount);
            Assert.True(1 == page.Items.Count, "Invalid size of page");
            Assert.True(DocumentType.ReportalIntegrationDashboard == page.Items[0].Type,
                "Type must be updated");
        }

        [Fact]
        public async Task PatchDocument_PublicMetadataIsNull_OldPublicMetadataIsUsed()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { PublicMetadata = null });

            Assert.Equal(publicMetadata, result.PublicMetadata);
        }

        [Fact]
        public async Task PatchDocument_PublicMetadataIsEmpty_PublicMetadataIsEmpty()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = "Public metadata" })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { PublicMetadata = "" });

            Assert.Equal(string.Empty, result.PublicMetadata);
        }

        [Fact]
        public async Task PatchDocument_PublicMetadata_NewPublicMetadata()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { PublicMetadata = publicMetadata });

            Assert.Equal(publicMetadata, result.PublicMetadata);
        }

        [Fact]
        public async Task PatchDocument_PrivateMetadataIsNull_OldPrivateMetadataIsUsed()
        {
            const string privateMetadata = "Private metadata";

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { PrivateMetadata = privateMetadata })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { PrivateMetadata = null });

            Assert.Equal(privateMetadata, result.PrivateMetadata);
        }

        [Fact]
        public async Task PatchDocument_PrivateMetadataIsEmpty_PrivateMetadataIsEmpty()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { PrivateMetadata = "Private metadata" })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { PrivateMetadata = "" });

            Assert.Equal(string.Empty, result.PrivateMetadata);
        }

        [Fact]
        public async Task PatchDocument_PrivateMetadata_NewPrivateMetadata()
        {
            const string privateMetadata = "Private metadata";

            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { PrivateMetadata = privateMetadata });

            Assert.Equal(privateMetadata, result.PrivateMetadata);
        }

        [Fact]
        public async Task PatchDocument_UserDoesNotHaveAccessToHub_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { HubId = Hub1.Id },
                HttpStatusCode.NotFound, $"Hub {Hub1.Id} not found");
        }

        [Fact]
        public async Task PatchDocument_Hub_DocumentHasHub()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync(new DocumentToCreateDto { HubId = Hub2.Id })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { HubId = Hub1.Id });

            Assert.Equal(Hub1.Id, result.HubId);
        }

        [Fact]
        public async Task PatchDocument_UserDoesNotHaveAccessToSurvey_NotFound()
        {
            await UseNormalUser2Async();
            var id = (await PostDocumentAsync()).Id;
            await PatchDocumentAsync(id, new DocumentPatchDto { LinkedSurveyId = Survey1.ProjectId },
                HttpStatusCode.NotFound, $"Survey {Survey1.ProjectId} not found");
        }

        [Fact]
        public async Task PatchDocument_Survey_DocumentHasSurvey()
        {
            await UseNormalUserAsync(); // NormalUser has access to Survey1 and Survey2
            var id = (await PostDocumentAsync(new DocumentToCreateDto { LinkedSurveyId = Survey1.ProjectId })).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { LinkedSurveyId = Survey2.ProjectId });

            Assert.Equal(Survey2.ProjectId, result.LinkedSurveyId);
        }

        [Fact]
        public async Task PatchDocument_OriginDocumentId_DocumentHasOriginDocumentId()
        {
            const int originDocumentId = 345;

            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            var result = await PatchDocumentAsync(id, new DocumentPatchDto { OriginDocumentId = originDocumentId });

            Assert.Equal(originDocumentId, result.OriginDocumentId);
        }

        #endregion

        #region DeleteDocument

        [Fact]
        public async Task DeleteDocument_DocumentDoesNotExist_NotFound()
        {
            await UseAdminAsync();
            await DeleteDocumentAsync(0, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteDocument_UserWithoutPermissions_Forbidden()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;

            await UseNormalUserAsync();
            await DeleteDocumentAsync(id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteDocument_UserHasPermissionManage_Success()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchUserPermissionsAsync(id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            await DeleteDocumentAsync(id); // successfully deleted
        }

        #endregion

        #region GetDocument

        [Fact]
        public async Task GetDocument_CreateDocument_ReturnsTheSameDocument()
        {
            await UseNormalUserAsync();
            var createdDocument = await PostDocumentAsync();
            var document = await GetDocumentAsync(createdDocument.Id);

            Assert.Equal(
                JsonConvert.SerializeObject(createdDocument),
                JsonConvert.SerializeObject(document));
        }

        [Fact]
        public async Task GetDocument_NotExistingDocument_NotFound()
        {
            await UseAdminAsync();
            await GetDocumentAsync(0, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDocument_UserHasNotPermission_Forbidden()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            await UseNormalUserAsync();
            await GetDocumentAsync(document.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDocument_NormalUserHasPermissionView_ReturnsDocument()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchUserPermissionsAsync(document.Id,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            document = await GetDocumentAsync(document.Id);

            Assert.NotNull(document);
        }

        [Fact]
        public async Task GetDocument_Enduser_Forbidden()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchEnduserPermissionsAsync(id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await UseEnduserAsync();
            await GetDocumentAsync(id, HttpStatusCode.Forbidden);
        }

        #endregion

        #region GetDocumentPublicMetadata

        [Fact]
        public async Task GetDocumentPublicMetadata_NormalUser_ReturnsMetadata()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata });
            var metadata = await GetDocumentPublicMetadata(document.Id);

            Assert.Equal(publicMetadata, metadata);
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_UnauthorizedUser_ReturnsMetadata()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata });

            UseUnauthorizedUser();
            var metadata = await GetDocumentPublicMetadata(document.Id);

            Assert.Equal(publicMetadata, metadata);
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_UnauthorizedUserAndPublicMetadataDoesNotExist_NotFound()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();

            UseUnauthorizedUser();
            await GetDocumentPublicMetadata(document.Id, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_NoAcceptHeader_PlainText()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata });
            UseUnauthorizedUser();
            var metadata = await GetDocumentPublicMetadata(document.Id, accept: null);

            Assert.Equal(publicMetadata, metadata);
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_AcceptTextPlain_PlainText()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata });
            // ReSharper disable once RedundantArgumentDefaultValue
            var metadata = await GetDocumentPublicMetadata(document.Id, accept: "text/plain");

            Assert.Equal(publicMetadata, metadata);
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_AcceptApplicationJson_Json()
        {
            const string publicMetadata = "Public metadata";

            await UseNormalUserAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata });
            var metadata = await GetDocumentPublicMetadata(document.Id, accept: "application/json");

            Assert.Equal($"\"{publicMetadata}\"", metadata);
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_AcceptApplicationJsonAndDocumentDoesNotExist_PlainText()
        {
            await GetDocumentPublicMetadata(long.MaxValue, HttpStatusCode.NotFound, accept: "application/json");
        }

        [Fact]
        public async Task GetDocumentPublicMetadata_AcceptTextPlainAndDocumentDoesNotExist_PlainText()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            await GetDocumentPublicMetadata(long.MaxValue, HttpStatusCode.NotFound, accept: "text/plain");
        }

        #endregion

        #region GetDocuments

        [Fact]
        public async Task GetDocuments_UnauthorizedUser_CorrectResponse()
        {
            UseUnauthorizedUser();
            var response = await Service.GetDocumentsAsync(null, null, null, null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains("Bearer realm", response.Headers.WwwAuthenticate.ToString());
        }

        [Fact]
        public async Task GetDocuments_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await GetDocumentsAsync(null,
                expectedStatusCode: HttpStatusCode.Forbidden); // Enduser has not access to documents
        }

        [Fact]
        public async Task GetDocuments_Admin_ReturnsAllDocuments()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            var id1 = document.Id;

            await UseNormalUserAsync();
            document = await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            var id2 = document.Id;

            await UseAdminAsync();
            var page = await GetDocumentsAsync(filter: $"Name eq '{Prefix}{name}'");

            Assert.Equal(2, page.ItemCount);
            Assert.Contains(page.Items, d => d.Id == id1);
            Assert.Contains(page.Items, d => d.Id == id2);
        }

        [Fact]
        public async Task GetDocuments_Admin_ValidPageAndLinks()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var id1 = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });

            var page = await GetDocumentsAsync(skip: 0, top: 1, orderBy: "Id asc", filter: $"Name eq '{Prefix}{name}'");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(1, page.ItemCount);
            Assert.Single(page.Items);
            Assert.Equal(id1, page.Items[0].Id);
            Assert.Null(page.Links.PreviousPage);
            var expected = $"/api/cdl/documents?$skip=1&$top=1&$orderby=Id asc&$filter=Name eq '{Prefix}{name}'";
            AssertValidLinks(expected, HttpUtility.UrlDecode(page.Links.NextPage));
        }

        [Fact]
        public async Task GetDocuments_Admin_PermissionManageForAllDocuments()
        {
            await UseAdminAsync();
            var page = await GetDocumentsAsync();

            Assert.True(page.Items.All(d => d.Permission == Permission.Manage));
        }

        [Fact]
        public async Task GetDocuments_ProsUser_ReturnsAllDocuments()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            var id1 = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;

            await UseNormalUserAsync();
            var id2 = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;

            await UseProsUserAsync();
            var page = await GetDocumentsAsync(filter: $"Name eq '{Prefix}{name}'");

            Assert.Equal(2, page.ItemCount);
            Assert.Contains(page.Items, d => d.Id == id1);
            Assert.Contains(page.Items, d => d.Id == id2);
        }

        [Fact]
        public async Task GetDocuments_ProsUser_PermissionManageForAllDocuments()
        {
            await UseProsUserAsync();
            var page = await GetDocumentsAsync();

            Assert.True(page.Items.All(d => d.Permission == Permission.Manage));
        }

        [Fact]
        public async Task GetDocuments_CompanyAdmin_ReturnsAvailableDocuments()
        {
            var name = GenerateRandomName();

            await UseAdminAsync();
            // Company administrator should not have access to this document because it's created under other company:
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            // Company administrator should have access to this document because he has granted permission:
            var id2 = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;
            await PatchUserPermissionsAsync(id2,
                new[] { new UserPermissionDto { Id = CompanyAdmin.Id, Permission = Permission.View } });
            // Company administrator should have access to this document because it belongs to his company:
            var id3 = (await PostDocumentAsync(new DocumentToCreateDto
                { Name = name, CompanyId = CompanyAdmin.CompanyId })).Id;

            await UseCompanyAdminAsync();
            // Company administrator should have access to this document because he is creator:
            var id4 = (await PostDocumentAsync(new DocumentToCreateDto { Name = name })).Id;

            var page = await GetDocumentsAsync(filter: $"Name eq '{Prefix}{name}'");

            Assert.Equal(3, page.ItemCount);
            Assert.Contains(page.Items, d => d.Id == id2);
            Assert.Contains(page.Items, d => d.Id == id3);
            Assert.Contains(page.Items, d => d.Id == id4);
        }

        [Fact]
        public async Task GetDocuments_NormalUser_ReturnsAvailableDocuments()
        {
            await UseAdminAsync();
            // User should not have access to this document because he has not permissions:
            var id1 = (await PostDocumentAsync()).Id;
            // User should have access to this document because he has granted permission:
            var id2 = (await PostDocumentAsync()).Id;
            await PatchUserPermissionsAsync(id2,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });
            // User should have access to this document because his company has granted permission:
            var id3 = (await PostDocumentAsync()).Id;
            await PutCompanyPermissionAsync(id3,
                new PermissionDto { Id = NormalUser.CompanyId, Permission = Permission.View });

            await UseNormalUserAsync();
            // User should have access to this document because he is creator:
            var id4 = (await PostDocumentAsync()).Id;

            var page = await GetDocumentsAsync(filter: $"Id eq {id1} or Id eq {id2} or Id eq {id3} or Id eq {id4}");

            Assert.Equal(3, page.ItemCount);
            Assert.Contains(page.Items, d => d.Id == id2);
            Assert.Contains(page.Items, d => d.Id == id3);
            Assert.Contains(page.Items, d => d.Id == id4);
        }

        [Fact]
        public async Task GetDocuments_TopAndSkipParametersAreSpecified_CorrectPage()
        {
            var name = GenerateRandomName();

            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });

            var page = await GetDocumentsAsync(skip: 1, top: 1, filter: $"contains(Name, '{name}')");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(1, page.ItemCount);
        }

        [Fact]
        public async Task GetDocuments_NormalUser_CorrectPage()
        {
            var name = GenerateRandomName();

            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });

            var page = await GetDocumentsAsync(filter: $"contains(Name, '{name}')");

            Assert.Equal(2, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
        }

        [Fact]
        public async Task GetDocuments_SortByIdAsc_CorrectOrder()
        {
            var name = GenerateRandomName();

            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });

            var documents = (await GetDocumentsAsync(orderBy: "Id asc", filter: $"Name eq '{Prefix}{name}'")).Items;

            Assert.Equal(2, documents.Count);
            Assert.True(documents[0].Id < documents[1].Id, "Incorrect sort order");
        }

        [Fact]
        public async Task GetDocuments_SortByIdDesc_CorrectOrder()
        {
            var name = GenerateRandomName();

            await UseNormalUserAsync();
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });
            await PostDocumentAsync(new DocumentToCreateDto { Name = name });

            var documents = (await GetDocumentsAsync(orderBy: "Id desc", filter: $"Name eq '{Prefix}{name}'")).Items;

            Assert.Equal(2, documents.Count);
            Assert.True(documents[0].Id > documents[1].Id, "Incorrect sort order");
        }

        #endregion

        #region PutPublishedRevision

        [Fact]
        public async Task PutPublishedRevision_PublishWrongRevision_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PutPublishedRevisionAsync(id, new RevisionToPublishDto { Id = long.MaxValue },
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PutPublishedRevision_TryToPublishRevisionOfOtherDocument_NotFound()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var otherDocumentId = (await PostDocumentAsync()).Id;
            var revisionOfOtherDocument = (await PostRevisionAsync(otherDocumentId)).Id;

            await PutPublishedRevisionAsync(id, new RevisionToPublishDto { Id = revisionOfOtherDocument },
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PutPublishedRevision_UserDoesNotHaveAccessToHub_NotFound()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync(
                new DocumentToCreateDto { HubId = Hub1.Id })).Id;
            var revisionId = (await PostRevisionAsync(id,
                new RevisionToCreateDto { Action = ActionToCreateRevision.CreateSnapshot })).Id;
            await PatchUserPermissionsAsync(id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();

            // By publishing document the user grants implicit read permissions to the document's hub
            // Therefore, NormalUser must have read access to the hub (he doesn't)
            await PutPublishedRevisionAsync(id, new RevisionToPublishDto { Id = revisionId },
                HttpStatusCode.NotFound, $"Hub {Hub1.Id} not found");
        }

        [Fact]
        public async Task PostRevision_UserDoesNotHaveAccessToSurvey_NotFound()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync(
                new DocumentToCreateDto { LinkedSurveyId = Survey1.ProjectId })).Id;
            var revisionId = (await PostRevisionAsync(id,
                new RevisionToCreateDto { Action = ActionToCreateRevision.CreateSnapshot })).Id;
            await PatchUserPermissionsAsync(id, new[]
                { new UserPermissionDto { Id = NormalUser2.Id, Permission = Permission.Manage } });

            await UseNormalUser2Async();

            // By publishing document the user grants implicit read permissions to the document's survey
            // Therefore, NormalUser2 must have read access to the survey (he doesn't)
            await PutPublishedRevisionAsync(id, new RevisionToPublishDto { Id = revisionId },
                HttpStatusCode.NotFound, $"Survey {Survey1.ProjectId} not found");
        }

        [Fact]
        public async Task PutPublishedRevision_Ok()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            var revision1 = (await PostRevisionAsync(id)).Id;
            var revision2 = (await PostRevisionAsync(id)).Id;

            // Second revision is published
            Assert.Equal(revision2, (await GetPublishedRevisionAsync(id)).Id);

            // Publish first revision
            await PutPublishedRevisionAsync(id, new RevisionToPublishDto { Id = revision1 });

            // First revision is published
            Assert.Equal(revision1, (await GetPublishedRevisionAsync(id)).Id);
        }

        #endregion

        #region GetPublishedRevision

        [Fact]
        public async Task GetPublishedRevision_EnduserAndDocumentHasNotPublishedRevision_Forbidden()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;

            await UseEnduserAsync();
            await GetPublishedRevisionAsync(id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPublishedRevision_EnduserHasNotAccessAndDocumentHasPublishedRevision_Forbidden()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PostRevisionAsync(id);

            await UseEnduserAsync();
            await GetPublishedRevisionAsync(id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPublishedRevision_EnduserAndDocumentHasPublishedRevision_ValidRevision()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchEnduserPermissionsAsync(id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
            var revision = await PostRevisionAsync(id);

            await UseEnduserAsync();
            var revision2 = await GetPublishedRevisionAsync(id);

            Assert.Equal(
                JsonConvert.SerializeObject(revision),
                JsonConvert.SerializeObject(revision2));
        }

        #endregion

        #region DeletePublishedRevision

        [Fact]
        public async Task DeletePublishedRevision_DocumentIsUnpublished()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;

            // Create published revision
            await PostRevisionAsync(id);

            // Document is published
            Assert.NotNull(await GetPublishedRevisionAsync(id));

            // Unpublish document
            await DeletePublishedRevisionAsync(id);

            // Document is unpublished
            await GetPublishedRevisionAsync(id, HttpStatusCode.NotFound);
        }

        #endregion
    }
}
