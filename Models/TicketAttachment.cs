using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeteorStrike.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        

        public string? Description { get; set; }


        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        //Foriegn Keys
        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        //Navigation Properties
        //Ticket
        public virtual Ticket? Ticket { get; set; }

        //BTUser
        public virtual BTUser? BTUser { get; set; }

    }
}
