using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.TestFramework.MessageBroker;
using JetBrains.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class RevisionTests : TestBase, IClassFixture<RevisionFixture>
    {
        private readonly RevisionFixture _fixture;

        public RevisionTests(SharedFixture sharedFixture, RevisionFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GET documents/revisions/published

        [Fact]
        public async Task GetPublishedRevisions_Enduser1_TwoRevisions()
        {
            await UseEnduserAsync();
            var revisions = (await GetPublishedRevisionsAsync(filter: "startswith(Name, 'RevisionTests')")).Items;

            // Enduser has access to published revisions from both documents
            Assert.Equal(2, revisions.Count);
            Assert.Contains(revisions, r => r.Name == "RevisionTests.doc1.rev3");
            Assert.Contains(revisions, r => r.Name == "RevisionTests.doc2.rev2");
            Assert.True(revisions[0].IsPublished);
            Assert.True(revisions[1].IsPublished);
        }

        [Fact]
        public async Task GetPublishedRevisions_NormalUser_OneRevision()
        {
            await UseNormalUserAsync();
            var revisions = (await GetPublishedRevisionsAsync(filter: "startswith(Name, 'RevisionTests')")).Items;

            // Normal user has access to published revisions from second document
            Assert.Single(revisions);
            Assert.Equal("RevisionTests.doc2.rev2", revisions[0].Name);
            Assert.True(revisions[0].IsPublished);
        }

        [Fact]
        public async Task GetPublishedRevisions_SearchByNameAndType_ValidResult()
        {
            await UseEnduserAsync();
            var revisions =
                (await GetPublishedRevisionsAsync(
                    filter: "startswith(Name, 'RevisionTests') and Type eq 'ReportalIntegrationDashboard'")).Items;

            Assert.Single(revisions);
            Assert.Equal("RevisionTests.doc2.rev2", revisions[0].Name);
            Assert.Equal(DocumentType.ReportalIntegrationDashboard, revisions[0].Type);
            Assert.True(revisions[0].IsPublished);
        }

        [Fact]
        public async Task GetPublishedRevisions_SortByDocumentIdAsc_CorrectOrder()
        {
            await UseEnduserAsync();
            var revisions =
                (await GetPublishedRevisionsAsync(orderBy: "DocumentId asc",
                    filter: "startswith(Name, 'RevisionTests')")).Items;

            var firstDocumentId = Math.Min(_fixture.Doc1.Id, _fixture.Doc2.Id);
            Assert.Equal(2, revisions.Count);
            Assert.Equal(firstDocumentId, revisions[0].DocumentId);
        }

        [Fact]
        public async Task GetPublishedRevisions_SortByDocumentIdDesc_CorrectOrder()
        {
            await UseEnduserAsync();
            var revisions =
                (await GetPublishedRevisionsAsync(orderBy: "DocumentId desc",
                    filter: "startswith(Name, 'RevisionTests')")).Items;

            var firstDocumentId = Math.Max(_fixture.Doc1.Id, _fixture.Doc2.Id);
            Assert.Equal(2, revisions.Count);
            Assert.Equal(firstDocumentId, revisions[0].DocumentId);
        }

        [Fact]
        public async Task GetPublishedRevisions_SortByWrongName_BadRequest()
        {
            await UseEnduserAsync();
            await GetPublishedRevisionsAsync(orderBy: "WRONG Desc", expectedStatusCode: HttpStatusCode.BadRequest);
        }

        #endregion

        #region GET documents/revisions/{revisionId}

        [Fact]
        public async Task GetRevision_UserHasPermissionView_ValidResult()
        {
            await UseProsUserAsync();
            var revision = await GetRevisionAsync(_fixture.Doc2Rev1.Id);

            Assert.NotNull(revision);
            Assert.Equal("RevisionTests.doc2.rev1", revision.Name);
            Assert.Equal("RevisionTests.doc2.rev1", revision.SourceCode);
            Assert.False(revision.IsPublished);
        }

        [Fact]
        public async Task GetRevision_UserWithoutPermissions_Forbidden()
        {
            await UseNormalUserAsync();
            await GetRevisionAsync(_fixture.Doc1Rev1.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetRevision_CorrectNumbers()
        {
            await UseAdminAsync();

            Assert.Equal(1, (await GetRevisionAsync(_fixture.Doc1Rev1.Id)).Number);
            Assert.Equal(2, (await GetRevisionAsync(_fixture.Doc1Rev2.Id)).Number);
            Assert.Equal(3, (await GetRevisionAsync(_fixture.Doc1Rev3.Id)).Number);
        }

        [Fact]
        public async Task GetRevision_CorrectFlagIsPublished()
        {
            await UseAdminAsync();

            Assert.False((await GetRevisionAsync(_fixture.Doc1Rev1.Id)).IsPublished);
            Assert.False((await GetRevisionAsync(_fixture.Doc1Rev2.Id)).IsPublished);
            Assert.True((await GetRevisionAsync(_fixture.Doc1Rev3.Id)).IsPublished);
        }

        #endregion

        #region POST documents/{id}/revisions

        [Fact]
        public async Task PostRevision_UserWithPermissionView_Forbidden()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchUserPermissionsAsync(id, new[]
            {
                new UserPermissionDto
                    { Id = NormalUser.Id, Permission = Permission.View }
            });

            await UseNormalUserAsync();
            await PostRevisionAsync(id, new RevisionToCreateDto { Name = "a", SourceCode = "b" },
                HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PostRevision_UserWithPermissionManage_RevisionBecomesPublished()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchUserPermissionsAsync(id, new[]
            {
                new UserPermissionDto
                    { Id = NormalUser.Id, Permission = Permission.Manage }
            });

            await UseNormalUserAsync();
            var revision = await PostRevisionAsync(id, new RevisionToCreateDto { Name = "a", SourceCode = "b" });

            Assert.True(revision.IsPublished);
            Assert.Equal(revision.Id, (await GetPublishedRevisionAsync(id)).Id);
        }

        [Fact]
        public async Task PostRevision_Snapshot_DocumentKeepsPreviousPublishedRevision()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            var publishedRevisionId = (await PostRevisionAsync(id)).Id;

            Assert.Equal(publishedRevisionId, (await GetPublishedRevisionAsync(id)).Id);

            await PostRevisionAsync(id, new RevisionToCreateDto { Action = ActionToCreateRevision.CreateSnapshot });

            Assert.Equal(publishedRevisionId, (await GetPublishedRevisionAsync(id)).Id);
        }

        [Fact]
        public async Task PostRevision_PublicMetadataIsNotSpecified_DocumentDataIsUsed()
        {
            await UseAdminAsync();
            const string publicMetadata = "{\"portalId\": 1}";
            var documentId = (await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = publicMetadata })).Id;
            var revision = await PostRevisionAsync(documentId);

            Assert.Equal(publicMetadata, revision.PublicMetadata);
        }

        [Fact]
        public async Task PostRevision_PublicMetadataIsEmptyString_PublicMetadataIsEmptyString()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync(new DocumentToCreateDto { PublicMetadata = "Public metadata" }))
                .Id;
            var revision = await PostRevisionAsync(documentId, new RevisionToCreateDto { PublicMetadata = "" });

            Assert.Equal(string.Empty, revision.PublicMetadata);
        }

        [Fact]
        public async Task PostRevision_PrivateMetadataIsNotSpecified_DocumentDataIsUsed()
        {
            await UseAdminAsync();
            const string privateMetadata = "PrivateMetadata";
            var documentId = (await PostDocumentAsync(
                new DocumentToCreateDto { PrivateMetadata = privateMetadata })).Id;
            var revision = await PostRevisionAsync(documentId);

            Assert.Equal(privateMetadata, revision.PrivateMetadata);
        }

        [Fact]
        public async Task PostRevision_PrivateMetadataIsEmptyString_PrivateMetadataIsNull()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync(
                new DocumentToCreateDto { PrivateMetadata = "Private metadata" })).Id;
            var revision = await PostRevisionAsync(documentId, new RevisionToCreateDto { PrivateMetadata = "" });

            Assert.Equal(string.Empty, revision.PrivateMetadata);
        }

        [Fact]
        public async Task PostRevision_PublicMetadataIsSpecified_NewDataIsAvailable()
        {
            await UseAdminAsync();
            const string publicMetadata = "{\"portalId\": 1}";
            var documentId = (await PostDocumentAsync(
                new DocumentToCreateDto { PublicMetadata = publicMetadata })).Id;

            const string newPublicMetadata = "{\"portalId\": 2}";
            var revision = await PostRevisionAsync(documentId, new RevisionToCreateDto
            {
                PublicMetadata = newPublicMetadata
            });

            Assert.Equal(newPublicMetadata, revision.PublicMetadata);
        }

        [Fact]
        public async Task PostRevision_PrivateMetadataIsSpecified_NewDataIsAvailable()
        {
            await UseAdminAsync();
            const string privateMetadata = "PrivateMetadata";
            var documentId = (await PostDocumentAsync(
                new DocumentToCreateDto { PrivateMetadata = privateMetadata })).Id;

            const string newPrivateMetadata = "new private metadata";
            var revision = await PostRevisionAsync(documentId, new RevisionToCreateDto
            {
                PrivateMetadata = newPrivateMetadata
            });

            Assert.Equal(newPrivateMetadata, revision.PrivateMetadata);
        }

        [Fact]
        public async Task PostRevision_SpecialDsl_MessageBrokerContainsMessage()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync(new DocumentToCreateDto { HubId = Hub1.Id })).Id;

            void Publish() => PostRevisionAsync(documentId, new RevisionToCreateDto
            {
                SourceCode = "config vault {}",
                PrivateMetadata = "{\"tags\":[\"vault\"]}"
            }).Wait();

            // Create and publish new revision with special SourceCode and wait for message
            MessageAsserter.WaitForMessage<CdlPublishedMessage>(
                Publish,
                "Confirmit.Cdl.Published",
                m => IsCorrectMessage(Hub1.Id, documentId, m),
                TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task PostRevision_UserDoesNotHaveAccessToHub_NotFound()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync(
                new DocumentToCreateDto { HubId = Hub1.Id })).Id;
            await PatchUserPermissionsAsync(documentId, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();

            // By publishing document the user grants implicit read permissions to the document's hub
            // Therefore, NormalUser must have read access to the hub (he doesn't)
            await PostRevisionAsync(documentId,
                expectedStatusCode: HttpStatusCode.NotFound, expectedErrorMessage: $"Hub {Hub1.Id} not found");
        }

        [Fact]
        public async Task PostRevision_UserDoesNotHaveAccessToSurvey_NotFound()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync(
                    new DocumentToCreateDto { LinkedSurveyId = Survey1.ProjectId })).Id;
            await PatchUserPermissionsAsync(documentId, new[]
                { new UserPermissionDto { Id = NormalUser2.Id, Permission = Permission.Manage } });

            // By publishing document the user grants implicit read permissions to the document's survey
            // Therefore, NormalUser2 must have read access to the survey (he doesn't)
            await UseNormalUser2Async();
            await PostRevisionAsync(documentId,
                expectedStatusCode: HttpStatusCode.NotFound, expectedErrorMessage: $"Survey {Survey1.ProjectId} not found");
        }

        [Fact]
        public async Task PostRevision_SourceCodeIsNull_SourceCodeFromDocumentIsUsed()
        {
            await UseNormalUserAsync();
            var documentId = (await PostDocumentAsync(new DocumentToCreateDto { SourceCode = "Document CDL" })).Id;

            var revision = await PostRevisionAsync(documentId, new RevisionToCreateDto { SourceCode = null });

            Assert.Equal("Document CDL", revision.SourceCode);
        }

        [Fact]
        public async Task PostRevision_SourceCodeIsEmpty_SourceCodeIsEmpty()
        {
            await UseNormalUserAsync();
            var documentId = (await PostDocumentAsync(new DocumentToCreateDto { SourceCode = "Document CDL" })).Id;

            var revision = await PostRevisionAsync(documentId, new RevisionToCreateDto { SourceCode = "" });

            Assert.Equal("", revision.SourceCode);
        }

        #endregion

        #region DELETE documents/revisions/{revisionId}

        [Fact]
        public async Task DeleteRevision_UserWithPermissionView_NotFound()
        {
            await UseAdminAsync();
            var id = (await PostDocumentAsync()).Id;
            await PatchUserPermissionsAsync(_fixture.Doc2.Id, new[]
                { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            await DeleteRevisionAsync(id, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteRevision_UserWithPermissionManage_DocumentBecomesUnpublished()
        {
            await UseNormalUserAsync();
            var id = (await PostDocumentAsync()).Id;

            // No published revision yet; GetDocumentPublishedRevision return NotFound
            await GetPublishedRevisionAsync(id, HttpStatusCode.NotFound);

            // Add revision
            var firstRevision = await PostRevisionAsync(id, new RevisionToCreateDto { Name = "a", SourceCode = "b" });

            // Now the document has published revision; this is first revision
            Assert.True(firstRevision.IsPublished);
            Assert.Equal(firstRevision.Id, (await GetPublishedRevisionAsync(id)).Id);

            // Add one more revision
            var revisionToDelete =
                await PostRevisionAsync(id, new RevisionToCreateDto { Name = "a", SourceCode = "b" });

            // Now this revision is published
            Assert.True(revisionToDelete.IsPublished);
            Assert.Equal(revisionToDelete.Id, (await GetPublishedRevisionAsync(id)).Id);

            // Delete latest revision
            await DeleteRevisionAsync(revisionToDelete.Id);

            // Document becomes unpublished
            await GetPublishedRevisionAsync(id, HttpStatusCode.NotFound);
        }

        #endregion

        #region GET documents/{id}/revisions

        [Fact]
        public async Task GetRevisions_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await GetRevisionsAsync(_fixture.Doc1.Id, null, expectedStatusCode: HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetRevisions_AllRevisionsOfFirstReport()
        {
            await UseAdminAsync();
            var revisions = (await GetRevisionsAsync(_fixture.Doc1.Id, orderBy: "Name asc")).Items;

            Assert.Equal(3, revisions.Count);
            Assert.Equal(_fixture.Doc1Rev1.Name, revisions[0].Name);
            Assert.Equal(1, revisions[0].Number);
            Assert.False(revisions[0].IsPublished);
            Assert.Equal(_fixture.Doc1Rev2.Name, revisions[1].Name);
            Assert.Equal(2, revisions[1].Number);
            Assert.False(revisions[1].IsPublished);
            Assert.Equal(_fixture.Doc1Rev3.Name, revisions[2].Name);
            Assert.Equal(3, revisions[2].Number);
            Assert.True(revisions[2].IsPublished);
        }

        [Fact]
        public async Task GetRevisions_UserWithPermissionView_AllRevisionsOfSecondReport()
        {
            await UseProsUserAsync();
            var revisions = (await GetRevisionsAsync(_fixture.Doc2.Id, orderBy: "Name desc")).Items;

            Assert.Equal(2, revisions.Count);
            Assert.Equal(_fixture.Doc2Rev2.Name, revisions[0].Name);
            Assert.Equal(_fixture.Doc2Rev1.Name, revisions[1].Name);
        }

        #endregion

        #region GET documents/{id}/revisions/published

        [Fact]
        public async Task GetPublishedRevision_EnduserAndDocumentHasNotPublishedRevision_Forbidden()
        {
            await UseEnduserAsync();
            await GetPublishedRevisionAsync(_fixture.Doc3.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPublishedRevision_EnduserAndDocumentHasPublishedRevision_ValidRevision()
        {
            await UseEnduserAsync();
            var revision = await GetPublishedRevisionAsync(_fixture.Doc1.Id);

            Assert.Equal(
                Newtonsoft.Json.JsonConvert.SerializeObject(_fixture.Doc1Rev3),
                Newtonsoft.Json.JsonConvert.SerializeObject(revision));
        }

        #endregion

        #region GET documents/{id}/revisions/published/public-metadata

        [Fact]
        public async Task GetPublishedRevisionPublicMetadata_UnauthorizedUser_Ok()
        {
            await UseAdminAsync();
            const string originalMetadata = "{\"portalId\":1}";
            var documentId = (await PostDocumentAsync(
                    new DocumentToCreateDto { PublicMetadata = originalMetadata })).Id;
            await PostRevisionAsync(documentId);

            UseUnauthorizedUser();
            var metadata = await GetPublicMetadataForPublishedRevisionAsync(documentId);

            Assert.True(originalMetadata == metadata,
                "Public metadata must be equal to original metadata");
        }

        [Fact]
        public async Task GetEmptyPublishedRevisionPublicMetadata_UnauthorizedUser_NotFound()
        {
            await UseAdminAsync();
            var documentId = (await PostDocumentAsync()).Id;
            await PostRevisionAsync(documentId);

            UseUnauthorizedUser();
            await GetPublicMetadataForPublishedRevisionAsync(documentId, HttpStatusCode.NotFound);
        }

        #endregion

        #region Helpers

        private static bool IsCorrectMessage(long expectedHubId, long expectedDocumentId, CdlPublishedMessage message)
        {
            return expectedHubId == message.HubId && expectedDocumentId == message.DocumentId;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class CdlPublishedMessage
        {
            public long? HubId { get; set; }
            public long DocumentId { get; set; }
            public string Token { get; set; }
        }

        #endregion
    }
}
