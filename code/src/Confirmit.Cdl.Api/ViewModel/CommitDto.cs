using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;

namespace Confirmit.Cdl.Api.ViewModel
{
    public enum Action
    {
        CreatePublished = 1,    // Create published revision
        CreateSnapshot = 2,     // Create snapshot revision
        Publish = 3,            // Publish the revision
        Unpublish = 4,          // unpublish the document
        Delete = 5,             // delete revision or document
        Restore = 6             // restore the document
    }

    public enum ActionToCreateRevision
    {
        CreatePublished,
        CreateSnapshot
    }

    [PublicAPI]
    public class CommitDto
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public long? RevisionId { get; set; }
        public int? RevisionNumber { get; set; }
        public Action Action { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public RevisionForCommitDto Revision { get; set; }
        public JObject Links { get; set; }
    }
}