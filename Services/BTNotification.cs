using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Models.Enums;
using MeteorStrike.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

		public async Task<List<Notification>> GetNotificationsByUserAsync(string? userId)
		{
			try
			{
				List<Notification> notifications = new();

				if (!string.IsNullOrEmpty(userId))
				{

					notifications = await _context.Notifications
												  .Where(n => n.RecipientId == userId || n.SenderId == userId)
												  .Include(n => n.Recipient)
												  .Include(n => n.Sender)
												  .ToListAsync();
				}

				return notifications;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> SendAdminEmailNotificationAsync(Notification? notification, string? emailSubject, int? companyId)
		{
			try
			{
				if (notification != null)
				{
					IEnumerable<string?> adminEmails = (await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId)).Select(u => u.Email);

					foreach (string adminEmail in adminEmails)
					{
						await _btMailService.SendEmailAsync(adminEmail, emailSubject!, notification.Message!);
					}
					return true;
				}
				return false;
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject)
		{
			try
			{
				if (notification != null) 
				{
					BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

					string? userEmail = btUser?.Email;

					if (userEmail != null)
					{
						await _btMailService.SendEmailAsync(userEmail, emailSubject!, notification.Message!);

						return true;
					}
				}
				return false;
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
