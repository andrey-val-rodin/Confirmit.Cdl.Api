using Confirmit.Cdl.Api.Authorization;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class RevisionToCreateDto
    {
        public string Name { get; set; }
        public ActionToCreateRevision Action { get; set; }
        public string SourceCode { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
    }

    [PublicAPI]
    public class RevisionToPublishDto
    {
        public long Id { get; set; }
    }

    [PublicAPI]
    public sealed class RevisionShortDto
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public DocumentType Type { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public sealed class AccessedRevisionDto
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public DocumentType Type { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime? Accessed { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public class RevisionDto
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public DocumentType Type { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }
        public string SourceCode { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
        public JObject Links { get; set; }
    }

    [PublicAPI]
    public class RevisionForCommitDto
    {
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string PublicMetadata { get; set; }
        public string PrivateMetadata { get; set; }
    }
}