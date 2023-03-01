using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;

namespace MeteorStrike.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public string? PropertyName { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? BTUserId { get; set; }

        //Foriegn Keys
        public int TicketId { get; set; }

        //Navigation Properties
        //Ticket
        public virtual Ticket? Ticket { get; set; }

        //User
        public virtual BTUser? BTUser { get; set; }
    }
}
