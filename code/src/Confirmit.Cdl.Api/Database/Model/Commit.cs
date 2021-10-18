using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class Commit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long DocumentId { get; set; }

        public long? RevisionId { get; set; }

        public int? RevisionNumber { get; set; }

        [Required]
        public byte Action { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [MaxLength(256)]
        public string CreatedByName { get; set; }
    }
}
