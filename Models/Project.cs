using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeteorStrike.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }

        public string? ImageFileType { get; set; }

        public bool Archived { get; set; }

        //Foriegn Keys
        public int CompanyId { get; set; }

        //Navigation Properties
        //Company
        public virtual Company? Company { get; set; }

        //ProjectPriority
        public virtual ProjectPriority? ProjectPriority { get; set; }  

        //Members
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        //Tickets
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
