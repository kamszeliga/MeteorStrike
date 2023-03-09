using Microsoft.AspNetCore.Mvc.Rendering;

namespace MeteorStrike.Models.ViewModels
{
	public class TicketMemberViewModel
	{
		public Ticket? Ticket { get; set; }

		public SelectList? UserList { get; set; }

		public string? DeveloperId { get; set; }
	}
}
