using Confirmit.Cdl.Api.Database.Contracts;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class Document : IArchivable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [Required]
        public byte Type { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [MaxLength(256)]
        public string CreatedByName { get; set; }

        [Required]
        public DateTime Modified { get; set; }

        [Required]
        public int ModifiedBy { get; set; }

        [MaxLength(256)]
        public string ModifiedByName { get; set; }

        public DateTime? Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public string DeletedByName { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(64)]
        public string CompanyName { get; set; }

        public string SourceCode { get; set; }

        public string SourceCodeEditOps { get; set; }

        public long? PublishedRevisionId { get; set; }

        [MaxLength(512)]
        public string PublicMetadata { get; set; }

        [MaxLength(512)]
        public string PrivateMetadata { get; set; }

        public long? HubId { get; set; }

        [MaxLength(50)]
        public string LinkedSurveyId { get; set; }

        public long? OriginDocumentId { get; set; }
    }
}
