using System.ComponentModel.DataAnnotations;

namespace MeteorStrike.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime Updated { get; set; }

        public bool Archived { get; set; }

        public bool ArchivedByProject { get; set; }

        //Foriegn Keys
        public int ProjectId { get; set; }

        public int TicketTypeId { get; set; }

        public int TicketStatusId { get; set; }

        public int TicketPriorityId { get; set; }

        public string? DeveloperUserId { get; set; }

        [Required]
        public string? SubmitterUserId { get; set; }

        //Navigation Properties
        //Project
        public virtual Project? Project { get; set; }

        //TicketPriority
        public virtual TicketPriority? TicketPriority { get; set; }

        //TicketType
        public virtual TicketType? TicketType { get; set; }

        //TicketStatus
        public virtual TicketStatus? TicketStatus { get; set; }

        //DeveloperUser
        public virtual BTUser? DeveloperUser { get; set; }

        //SubmitterUser
        public virtual BTUser? SubmitterUser { get; set; }

        //Comments
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        //Attachments
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        //History
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
