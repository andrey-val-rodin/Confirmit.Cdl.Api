using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.MessageBroker.Publish.Sdk;
using Confirmit.NetCore.Common;
using System;

namespace Confirmit.Cdl.Api.Services
{
    public enum EventKind
    {
        Document,
        Revision,
        All
    }

    public enum EventAction
    {
        Created,
        Updated,
        Deleted,
        Published,
        Dismissed,
        All
    }

    public class DocumentEvent
    {
        public int CompanyId;
        public int UserId;
        public Uri DocumentUrn;
        public Uri PublicRevisionUrn;
        public DateTimeOffset IssuedAt;
    }

    public class RevisionEvent
    {
        public int CompanyId;
        public int UserId;
        public Uri RevisionUrn;
        public DateTimeOffset IssuedAt;
    }

    public class EventService
    {
        public const string ExchangerName = "Confirmit.Document";
        private const string DocumentMessageType = "DocumentEvent";
        private const string RevisionMessageType = "DocumentRevisionEvent";

        private static readonly ConfirmitMessageBrokerPublisher Publisher = new ConfirmitMessageBrokerPublisher();

        private readonly IConfirmitScopeContext _scopeContext;

        public EventService(IConfirmitScopeContext scopeContext)
        {
            _scopeContext = scopeContext;
        }

        public void Issue(Document document, Revision revision, EventKind kind, EventAction action, int userId)
        {
            Publisher.Publish(ExchangerName, new RevisionEvent
                {
                    CompanyId = document.CompanyId,
                    RevisionUrn = CreateRevisionUrn(document.Id, revision.Id),
                    UserId = userId,
                    IssuedAt = DateTimeOffset.UtcNow
                },
                new MessageCorrelationId(_scopeContext.CorrelationId),
                CreateTopic((DocumentType) document.Type, kind, action),
                RevisionMessageType
            );
        }

        public void Issue(Document document, EventAction action, int userId)
        {
            Publisher.Publish(ExchangerName, new DocumentEvent
                {
                    CompanyId = document.CompanyId,
                    DocumentUrn = CreateDocumentUrn(document.Id),
                    PublicRevisionUrn = action != EventAction.Dismissed && document.PublishedRevisionId.HasValue
                        ? CreateRevisionUrn(document.Id, document.PublishedRevisionId.Value)
                        : null,
                    UserId = userId,
                    IssuedAt = DateTimeOffset.UtcNow
                },
                new MessageCorrelationId(_scopeContext.CorrelationId),
                CreateTopic((DocumentType) document.Type, EventKind.Document, action),
                DocumentMessageType
            );
        }

        public void Issue(Document document, string token)
        {
            Publisher.Publish(
                "Confirmit.Cdl.Published",
                new
                {
                    document.HubId,
                    DocumentId = document.Id,
                    Token = token
                },
                new MessageCorrelationId(_scopeContext.CorrelationId),
                "vault");
        }

        public static string CreateTopic(DocumentType documentType, EventKind eventKind, EventAction eventAction)
        {
            var kind = eventKind == EventKind.All ? "*" : eventKind.ToString().ToLower();
            var action = eventAction == EventAction.All ? "*" : eventAction.ToString().ToLower();
            var documentTypeString =
                documentType == DocumentType.NotSpecified ? "*" : documentType.ToString().ToLower();
            return
                $"confirmit.{documentTypeString}.cdl.{kind}.{action}";
        }

        private static Uri CreateDocumentUrn(long documentId)
        {
            return new Uri($"urn:confirmit:cdl:document:{documentId}");
        }

        private static Uri CreateRevisionUrn(long documentId, long revisionId)
        {
            return new Uri($"urn:confirmit:cdl:document:{documentId}:revision:{revisionId}");
        }
    }
}
