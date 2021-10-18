using Confirmit.Cdl.Api.Database.Contracts;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class User : IUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [MaxLength(254)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string FirstName { get; set; }

        [MaxLength(64)]
        public string LastName { get; set; }

        public int OrganizationId { get; set; }

        #region Explicit IEntity interface implementations

        long IEntity.Id => Id;

        #endregion
    }
}
