using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class DocumentAlias
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Namespace { get; set; }

        [Required]
        [MaxLength(80)]
        public string Alias { get; set; }

        [Required]
        public long DocumentId { get; set; }
    }
}
