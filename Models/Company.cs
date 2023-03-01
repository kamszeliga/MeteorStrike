

using Azure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeteorStrike.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        // Foriegn Keys
        // N/A

        //Navigation properties

        //Projects
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        //Members
        public virtual ICollection<BTUser> BTUsers { get; set; } = new HashSet<BTUser>();

        //Invites
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
