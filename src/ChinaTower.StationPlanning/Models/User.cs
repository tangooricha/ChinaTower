using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ChinaTower.StationPlanning.Models
{
    public class User : IdentityUser
    {
        [ForeignKey("Avatar")]
        public Guid? BlobId { get; set; }

        public Blob Avatar { get; set; }
    }
}
