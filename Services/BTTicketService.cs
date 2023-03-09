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

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;

        }

        public Task AddTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task<Ticket> GetTicketAsync(int? ticketId)
        {
            try
            {
                return await _context.Tickets
                                     .Include(t => t.DeveloperUser)
                                     .Include(t => t.SubmitterUser)
                                     .Include(t => t.Project)
                                     .Include(t => t.TicketPriority)
                                     .Include(t => t.TicketStatus)
                                     .Include(t => t.TicketType)
                                     .Include(t => t.Comments)
                                     .Include(t => t.Attachments)
                                     .Include(t => t.History)
                                     .FirstOrDefaultAsync(t => t.Id == ticketId);
                                     
            }
            catch (Exception)
            {

                throw;
            }
        }
       
        public async Task<IEnumerable<Ticket>> GetTicketsAsync(int projectId)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                       .Where(t => t.Archived == false && t.ProjectId == projectId)
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

        public Task UpdateTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
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

	}
}
