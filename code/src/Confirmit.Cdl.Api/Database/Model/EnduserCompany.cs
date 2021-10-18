using Confirmit.Cdl.Api.Database.Contracts;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class EnduserCompany : IOrganization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        #region Explicit IEntity interface implementations

        long IEntity.Id => Id;

        #endregion
    }
}
