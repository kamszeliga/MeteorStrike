using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Models.Enums;
using MeteorStrike.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.Design;

namespace MeteorStrike.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _btRolesService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService btRolesService)
        {
            _context = context;
            _btRolesService = btRolesService;
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            _context.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public Task ArchiveTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task<Ticket> GetTicketAsync(int? ticketId)
        {
            try
            {

                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.Project)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.Comments)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .Include(t => t.History)
                                               .Include(t => t.Attachments)
                                               .FirstOrDefaultAsync(m => m.Id == ticketId);
                return ticket;




            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                       .Where(t => t.Archived == false && t.Project.CompanyId == companyId)
                                                       .Include(t => t.DeveloperUser)
                                                       .Include(t => t.Project)
                                                       .Include(t => t.SubmitterUser)
                                                       .Include(t => t.TicketPriority)
                                                       .Include(t => t.TicketStatus)
                                                       .Include(t => t.TicketType)
                                                       .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            Ticket? ticket = await _context.Tickets
                                   .Include(t => t.Project)
                                       .ThenInclude(p => p.Company)
                                   .Include(t => t.Attachments)
                                   .Include(t => t.Comments)
                                   .Include(t => t.DeveloperUser)
                                   .Include(t => t.History)
                                   .Include(t => t.SubmitterUser)
                                   .Include(t => t.TicketPriority)
                                   .Include(t => t.TicketStatus)
                                   .Include(t => t.TicketType)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);
            return ticket;
        }

        public async Task AddTicketComment(TicketComment ticketComment)
        {
            _context.Add(ticketComment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        //-------------------------------------------------

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<TicketPriority>> GetTicketPriorities()
        {
            IEnumerable<TicketPriority> ticketPriorities = await _context.TicketPriorities.ToListAsync();
            return ticketPriorities;
        }

        public async Task<IEnumerable<TicketStatus>> GetTicketStatuses()
        {
            IEnumerable<TicketStatus> ticketStatuses = await _context.TicketStatuses.ToListAsync();

            return ticketStatuses;
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypes()
        {
            IEnumerable<TicketType> ticketTypes = await _context.TicketTypes.ToListAsync();

            return ticketTypes;
        }

        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(string? userId, int? companyId)
        {
            IEnumerable<Ticket> tickets = await _context.Tickets.Where(t => t.SubmitterUserId == userId || t.DeveloperUserId == userId)
                                                                .Include(t => t.Project)
                                                                .Include(t => t.SubmitterUser)
                                                                .Include(t => t.DeveloperUser)
                                                                .Include(t => t.Comments)
                                                                .Include(t => t.TicketPriority)
                                                                .Include(t => t.TicketStatus)
                                                                .Include(t => t.TicketType)
                                                                .Include(t => t.History)
                                                                .Include(t => t.Attachments)
                                                                .ToListAsync();
            return tickets; 
        }

		public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
		{
			try
			{
				TicketAttachment ticketAttachment = await _context.TicketAttachments
																  .Include(t => t.BTUser)
																  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
				return ticketAttachment;
			}
			catch (Exception)
			{

				throw;
			}
		}

        public async Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync(BTUser? user)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();
                //Get all unassigned tickets for the admin in the company
                if (await _btRolesService.IsUserInRoleAsync(user!, nameof(BTRoles.Admin)))
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == user!.CompanyId && t.Archived == false && t.DeveloperUser == null)
                                            .ToListAsync();
                }
                if (await _btRolesService.IsUserInRoleAsync(user!, nameof(BTRoles.ProjectManager)))
                {
                    tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == user!.CompanyId && t.Archived == false && t.DeveloperUser == null && user!.Projects!.Contains(t.Project))
                                            .ToListAsync();
                }
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
