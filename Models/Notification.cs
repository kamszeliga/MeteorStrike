using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Pkcs;

namespace MeteorStrike.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Title { get; set; }

        public string? Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public int NotificationTypeId { get; set; }

        [Display(Name = "Viewed")]
        public bool HasBeenViewed { get; set; }

        //Foriegn Keys

        public int ProjectId { get; set; }

        public int TicketId { get; set; }

        public string? SenderId { get; set; }

        public string? RecipientId { get; set; }

        //Navigation Properties
        //NotificationType
        public virtual NotificationType? NotificationType { get; set; }

        //Ticket
        public virtual Ticket? Ticket { get; set; }

        //Project
        public virtual Project? Project { get; set; }

        //Sender
        public virtual BTUser? Sender { get; set; }

        //Recipient
        public virtual BTUser? Recipient { get; set; }

        //public virtual Category? Category { get; set; } // one to many

        //public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>(); //many to many
    }
}
