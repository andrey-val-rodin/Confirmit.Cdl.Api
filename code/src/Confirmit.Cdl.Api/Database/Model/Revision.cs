using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class Revision
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long DocumentId { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string SourceCode { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [MaxLength(256)]
        public string CreatedByName { get; set; }

        [MaxLength(512)]
        public string PublicMetadata { get; set; }

        [MaxLength(512)]
        public string PrivateMetadata { get; set; }
    }
}
