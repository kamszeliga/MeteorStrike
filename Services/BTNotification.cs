using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Models.Enums;
using MeteorStrike.Services.Interfaces;

namespace MeteorStrike.Services
{
	public class BTNotification : IBTNotifications
	{
		private readonly ApplicationDbContext _context;
		private readonly BTRolesService _btRolesService;
		private readonly BTEmailService _btMailService;

		public BTNotification(ApplicationDbContext context,
								BTRolesService btRolesService,
								BTEmailService btMailService)
		{
			_context = context;
			_btRolesService = btRolesService;
			_btMailService = btMailService;

		}



		public async Task AddNotificationAsync(Notification? notification)
		{
			try
			{
				if (notification != null)
				{
					await _context.AddAsync(notification);
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task AdminNotificationAsync(Notification? notification, int? companyId)
		{
			try
			{
				if (notification != null)
				{
					IEnumerable<string> adminIds = (await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId)).Select(u => u.Id);

					foreach (string adminId in adminIds) 
					{
						notification.Id = 0;
						notification.RecipientId = adminId;

						await _context.AddAsync(notification);

					}

					await _context.SaveChangesAsync();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public Task<List<Notification>> GetNotificationsByUserAsync(string? userId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> SendAdminEmailNotificationAsync(Notification? notification, string? emailSubject, int? companyId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject)
		{
			throw new NotImplementedException();
		}
	}
}
