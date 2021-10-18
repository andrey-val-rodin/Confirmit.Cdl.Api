using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class EventTests : TestBase
    {
        private const int TimeoutInSec = 15;
        private const string CdlName = Prefix + "Test Automation Name";
        private const string CdlSourceCode = "Action Defintion CDL";

        public EventTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        #region Document

        [Fact]
        public async Task CreateDocument_Automation_IssuesDocumentCreatedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.Automation,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Created,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task DeleteDocument_ProgramDashboard_IssuesDocumentDeletedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Deleted,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    await DeleteDocumentAsync(document.Id);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task RestoreDocument_ReportalIntegrationDashboard_IssuesDocumentCreatedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ReportalIntegrationDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Created,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    await DeleteDocumentAsync(document.Id);
                    await RestoreArchivedDocumentAsync(document.Id);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task PatchDocument_DataFlow_IssuesDocumentUpdatedEvent()
        {
            await UseAdminAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = Admin
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Updated,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = arrange.DocumentType,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\", \"Hub\": {Hub1} }}"
                    });

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task PatchDocument_ChangeType_IssuesDocumentDeletedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Deleted,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = DocumentType.Automation,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
                    });

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task PatchDocument_ChangeType_IssuesDocumentCreatedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                DocumentType.Automation,
                EventAction.Created,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = DocumentType.Automation,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
                    });

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        #endregion

        #region Revision

        [Fact]
        public async Task CreateRevision_Automation_IssuesRevisionCreatedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.Automation,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Created,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task DeleteRevision_DataTemplate_IssuesRevisionDeletedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataTemplate,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Deleted,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await DeleteRevisionAsync(revision.Id);

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        #endregion

        #region Publishing & Revision 

        [Fact]
        public async Task PublishRevision_ProgramDashboard_IssuesRevisionPublishedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Published,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task PublishRevision_ProgramDashboard_IssuesDocumentPublishedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Published,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task UnPublishRevision_ProgramDashboard_IssuesRevisionDismissedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeletePublishedRevisionAsync(document.Id);

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task UnPublishRevision_ProgramDashboard_IssuesDocumentDismissedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeletePublishedRevisionAsync(document.Id);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task DeletePublishedRevision_ReportalIntegrationDashboard_IssuesRevisionDismissedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ReportalIntegrationDashboard,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteRevisionAsync(revision.Id);

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task DeletePublishedRevision_ReportingDashboard_IssuesRevisionDeletedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ReportingDashboard,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Deleted,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteRevisionAsync(revision.Id);

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task DeletePublishedRevision_ProgramDashboard_IssuesDocumentDismissedEvent()
        {
            await UseNormalUserAsync();
            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteRevisionAsync(revision.Id);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        #endregion

        #region Publishing & Document

        [Fact]
        public async Task DeletePublishedDocument_ProgramDashboard_IssuesDocumentDismissEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ProgramDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteDocumentAsync(document.Id);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task DeletePublishedDocument_ReportalIntegrationDashboard_IssuesRevisionDismissedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ReportalIntegrationDashboard,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteDocumentAsync(document.Id);

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task RestorePublishedDocument_ReportalIntegrationDashboard_IssuesDocumentPublishedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ReportalIntegrationDashboard,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Published,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteDocumentAsync(document.Id);
                    await RestoreArchivedDocumentAsync(document.Id);

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task RestorePublishedDocument_ReportalIntegrationDashboard_IssuesRevisionPublishedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.ReportalIntegrationDashboard,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Published,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await DeleteDocumentAsync(document.Id);
                    await RestoreArchivedDocumentAsync(document.Id);

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task PatchPublishedDocument_ChangeType_IssuesDocumentDismissedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = DocumentType.Automation,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
                    });

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = null
                    };
                });
        }

        [Fact]
        public async Task PatchPublishedDocument_ChangeType_IssuesDocumentPublishedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = NormalUser
            };

            await AssertDocumentEventIssuedAsync(
                EventAction.Published,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = DocumentType.Automation,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
                    });

                    return new DocumentEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        DocumentUrn = new Uri("urn:confirmit:cdl:document:" + document.Id),
                        PublicRevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task PatchPublishedDocument_ChangeType_IssuesRevisionDismissedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Dismissed,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = DocumentType.Automation,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
                    });

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        [Fact]
        public async Task PatchPublishedDocument_ChangeType_IssuesRevisionPublishedEvent()
        {
            await UseNormalUserAsync();

            var arrangeSet = new Arrange
            {
                DocumentType = DocumentType.DataFlow,
                User = NormalUser
            };

            await AssertRevisionEventIssuedAsync(
                EventAction.Published,
                arrangeSet,
                // act
                async arrange =>
                {
                    var document = await CreateTestDocumentAsync(arrange.DocumentType);
                    var revision = await PostRevisionAsync(document.Id);
                    await PutPublishedRevisionAsync(document.Id, new RevisionToPublishDto { Id = revision.Id });
                    await PatchDocumentAsync(document.Id, new DocumentPatchDto
                    {
                        Name = CdlName + "modified name " + Guid.NewGuid(),
                        Type = DocumentType.Automation,
                        SourceCode = CdlSourceCode,
                        SourceCodeEditOps = "New source code edit ops",
                        PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
                    });

                    return new RevisionEvent
                    {
                        CompanyId = arrange.User.CompanyId,
                        UserId = arrange.User.Id,
                        RevisionUrn = new Uri($"urn:confirmit:cdl:document:{document.Id}:revision:{revision.Id}")
                    };
                });
        }

        #endregion

        #region Helpers

        private async Task<DocumentDto> CreateTestDocumentAsync(DocumentType documentType)
        {
            return await PostDocumentAsync(new DocumentToCreateDto
            {
                Name = CdlName,
                Type = documentType,
                SourceCode = CdlSourceCode,
                PrivateMetadata = $"{{\"LinkedSurveyId\": \"{Survey1}\"}}"
            });
        }

        private static async Task AssertDocumentEventIssuedAsync(
            EventAction eventAction,
            Arrange arrangeSet,
            Func<Arrange, Task<DocumentEvent>> action)
        {
            await AssertDocumentEventIssuedAsync(
                arrangeSet.DocumentType,
                eventAction,
                arrangeSet,
                action);
        }

        private static async Task AssertDocumentEventIssuedAsync(
            DocumentType documentType,
            EventAction eventAction,
            Arrange arrangeSet,
            Func<Arrange, Task<DocumentEvent>> action)
        {
            var startAt = DateTimeOffset.UtcNow.AddSeconds(-10);

            var topic = EventService.CreateTopic(
                documentType,
                EventKind.Document,
                eventAction);

            await MessageBusAssert.EventReceivedAsync<DocumentEvent, Arrange, DocumentEvent>(
                EventService.ExchangerName,
                topic,
                TimeSpan.FromSeconds(TimeoutInSec),

                // arrange
                arrangeSet,

                // act
                action,

                // assert
                (msg, arrange, expectedEvent) =>
                {
                    Assert.True(msg != null, "Message Bus event is null");
                    Assert.True(topic == msg.RoutingKey, "Routing Key does not match");
                    Assert.True(msg.Content != null, "Message bus event content is null");
                    EventAssert.AreEqual(expectedEvent, msg.Content);
                    Assert.True(startAt <= msg.Content.IssuedAt, $"Wrong time {startAt} is greater than {msg.Content.IssuedAt}");
                    return true;
                }
            );
        }

        private static async Task AssertRevisionEventIssuedAsync(
            EventAction eventAction,
            Arrange arrangeSet,
            Func<Arrange, Task<RevisionEvent>> action)
        {
            await AssertRevisionEventIssuedAsync(
                arrangeSet.DocumentType,
                eventAction,
                arrangeSet,
                action);
        }

        private static async Task AssertRevisionEventIssuedAsync(
            DocumentType documentType,
            EventAction eventAction,
            Arrange arrangeSet,
            Func<Arrange, Task<RevisionEvent>> action)
        {
            var startAt = DateTimeOffset.UtcNow.AddSeconds(-10);

            var topic = EventService.CreateTopic(
                documentType,
                EventKind.Revision,
                eventAction);

            await MessageBusAssert.EventReceivedAsync<RevisionEvent, Arrange, RevisionEvent>(
                EventService.ExchangerName,
                topic,
                TimeSpan.FromSeconds(TimeoutInSec),

                // arrange
                arrangeSet,

                // act
                action,

                // assert
                (msg, arrange, expectedEvent) =>
                {
                    Assert.True(msg != null, "Message Bus event is null");
                    Assert.True(topic == msg.RoutingKey, "Routing Key does not match");
                    Assert.True(msg.Content != null, "Message bus event content is null");
                    EventAssert.AreEqual(expectedEvent, msg.Content);
                    Assert.True(startAt <= msg.Content.IssuedAt, $"Wrong time {startAt} is greater than {msg.Content.IssuedAt}");
                    return true;
                }
            );
        }

        private class Arrange
        {
            public DocumentType DocumentType;
            public User User;
        }

        #endregion
    }
}
