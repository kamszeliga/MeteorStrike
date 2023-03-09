using MeteorStrike.Models;
using System.ComponentModel.Design;

namespace MeteorStrike.Services.Interfaces
{
	public interface IBTCompanyService
	{
		public Task<Company> GetCompanyInfoAsync(int? companyId);

		public Task<List<BTUser>> GetMembersAsync(int? companyId);
	}
}
