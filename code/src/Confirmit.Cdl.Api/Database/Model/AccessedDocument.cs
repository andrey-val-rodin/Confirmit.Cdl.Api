using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class AccessedDocument
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public bool IsUser { get; set; }

        [Required]
        public DateTime Accessed { get; set; }
    }
}
