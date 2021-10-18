using Confirmit.Cdl.Api.Authorization;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class DocumentToCreateDto
    {
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public int CompanyId { get; set; }
        public string SourceCode { get; set; }
        public long? HubId { get; set; }
        public string LinkedSurveyId { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public long? OriginDocumentId { get; set; }
    }

    [PublicAPI]
    public class DocumentPatchDto
    {
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public string SourceCode { get; set; }
        public int CompanyId { get; set; }
        public string SourceCodeEditOps { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public long? HubId { get; set; }
        public string LinkedSurveyId { get; set; }
        public long? OriginDocumentId { get; set; }
    }

    [PublicAPI]
    public sealed class DocumentShortDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime Modified { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public DateTime? Deleted { get; set; }
        public int? DeletedBy { get; set; }
        public string DeletedByName { get; set; }
        public long? PublishedRevisionId { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public long? HubId { get; set; }
        public string LinkedSurveyId { get; set; }
        public long? OriginDocumentId { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public sealed class AccessedDocumentDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime? Accessed { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime Modified { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public long? PublishedRevisionId { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public long? HubId { get; set; }
        public string LinkedSurveyId { get; set; }
        public long? OriginDocumentId { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public class DocumentDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string SourceCode { get; set; }
        public string SourceCodeEditOps { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime Modified { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public DateTime? Deleted { get; set; }
        public int? DeletedBy { get; set; }
        public string DeletedByName { get; set; }
        public long? PublishedRevisionId { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public long? HubId { get; set; }
        public string LinkedSurveyId { get; set; }
        public long? OriginDocumentId { get; set; }
        public JObject Links { get; set; }
    }
}