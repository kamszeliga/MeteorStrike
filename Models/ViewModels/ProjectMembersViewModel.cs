using Microsoft.AspNetCore.Mvc.Rendering;

namespace MeteorStrike.Models.ViewModels
{
	public class ProjectMembersViewModel
	{
		public Project? Project { get; set; }

		public MultiSelectList? UserList { get; set; }

		public List<string>? SelectedMembers { get; set; }
	}
}
