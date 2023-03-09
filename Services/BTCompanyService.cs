using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeteorStrike.Services
{
	public class BTCompanyService : IBTCompanyService
	{

		private readonly ApplicationDbContext _context;
		private readonly IBTRolesService _btRolesService;

		public BTCompanyService(ApplicationDbContext context,
								IBTRolesService btRolesService)
		{
			_context = context;
			_btRolesService = btRolesService;
		}


		public async Task<Company> GetCompanyInfoAsync(int? companyId)
		{
			try
			{
				Company? company = new();

				if (companyId != null)
				{ 
				  company = await _context.Companies
												.Include(c => c.Members)
												.Include(c => c.Projects)
												.Include(c => c.Invites)
												.FirstOrDefaultAsync(c => c.Id == companyId);
				}
												
				return company!;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<List<BTUser>> GetMembersAsync(int? companyId)
		{
			try
			{
				List<BTUser> members = new();

				members = await _context.Users.Where(u => u.CompanyId == companyId)
												.ToListAsync();

				return members;
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
