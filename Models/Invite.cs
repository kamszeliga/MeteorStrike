using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeteorStrike.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string InviteeFullName { get { return $"{InviteeFirstName} {InviteeLastName}"; } }


        public string? Message { get; set; }

        public bool IsValid { get; set; }

        //Foriegn Keys
        public int CompanyId { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }
        public string? InviteeId { get; set; }

        //Navigation
        //Company
        public virtual Company? Company { get; set; }

        //Project
        public virtual Project? Project { get; set; }

        //Invitor
        public virtual BTUser? Invitor { get; set; }

        //Invitee
        public virtual BTUser? Invitee { get; set; }

    }
}
