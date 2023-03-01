using System.ComponentModel.DataAnnotations;

namespace MeteorStrike.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        public string? Comment { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        //Foreign Keys
        public int TicketId { get; set; }

        public string? BTUserId { get; set; }

        //Navigation
        //Ticket
        public virtual Ticket? Ticket { get; set; }

        //User
        public virtual BTUser? BTUser { get; set; }

    }
}
