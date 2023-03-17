using DailyRoarBlog.Data;
using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeteorStrike.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId)
        {
            try
            {
                if (oldTicket == null && newTicket != null)
                {
                    TicketHistory history = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = string.Empty,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        BTUserId = userId,
                        Description = "New Ticket Created"
                    };

                    try
                    {
                        await _context.TicketHistories.AddAsync(history);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    };


                }
                else if (oldTicket != null && newTicket != null)
                { 
                    // Check Ticket Title
                    if (oldTicket.Title != newTicket.Title)
                    {
                        TicketHistory history = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            BTUserId = userId,
                            Description = $"Ticket title updated from {oldTicket.Title} to {newTicket.Title}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    // Check Ticket Description
                    if (oldTicket.Description != newTicket.Description)
                    {
                        TicketHistory history = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            BTUserId = userId,
                            Description = $"Ticket description updated from {oldTicket.Description} to {newTicket.Description}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    // Check Ticket Priority 
                    if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                    {
                        TicketHistory history = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "TicketPriority",
                            OldValue = oldTicket.TicketPriority?.Name,
                            NewValue = newTicket.TicketPriority?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            BTUserId = userId,
                            Description = $"Ticket priority updated from {oldTicket.TicketPriority?.Name} to {newTicket.TicketPriority?.Name}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    // Check Ticket Status 
                    if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                    {
                        TicketHistory history = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "TicketStatus",
                            OldValue = oldTicket.TicketStatus?.Name,
                            NewValue = newTicket.TicketStatus?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            BTUserId = userId,
                            Description = $"Ticket status updated from {oldTicket.TicketStatus?.Name} to {newTicket.TicketStatus?.Name}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    // Check Ticket Type 
                    if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                    {
                        TicketHistory history = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "TicketType",
                            OldValue = oldTicket.TicketType?.Name,
                            NewValue = newTicket.TicketType?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            BTUserId = userId,
                            Description = $"Ticket type updated from {oldTicket.TicketType?.Name} to {newTicket.TicketType?.Name}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    // Check Ticket Developer
                    if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                    {
                        TicketHistory history = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "DeveloperUser",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket.DeveloperUser?.FullName,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            BTUserId = userId,
                            Description = string.Empty,
                        };

                        if (oldTicket.DeveloperUserId != null)
                        {
                            history.Description = $"Ticket priority updated from {oldTicket.DeveloperUser?.FullName} to {newTicket.DeveloperUser?.FullName}";
                        }
                        else
                        {
                            history.Description = $"Ticket assigned to {newTicket.DeveloperUser?.FullName}";
                        }

                        await _context.TicketHistories.AddAsync(history);

                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddHistoryAsync(int? ticketId, string? model, string? userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);

                string description = model!.ToLower().Replace("ticket", "");

                description = $"New {description} added to ticket: {ticket?.Title}";

                if (ticket != null)
                {

                    TicketHistory history = new()
                    {
                        TicketId = ticket!.Id,
                        PropertyName = model,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        BTUserId = userId,
                        Description = description
                    };
                    await _context.TicketHistories.AddAsync(history) ;
                    await _context.SaveChangesAsync();

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketHistoriesAsync(int? projectId, int? companyId)
        {
            throw new NotImplementedException();

        }

        public async Task<List<TicketHistory>> GetCompanyTicketHistoriesAsync(int? companyId)
        {
            Company? company = await _context.Companies
                                                 .Include(c => c.Projects)
                                                 .ThenInclude(p => p.Tickets)
                                                 .ThenInclude(t => t.History)
                                                 .FirstOrDefaultAsync();

            IEnumerable<Ticket> companyTickets = company
                                                .Projects.SelectMany(p => p.Tickets.Where(t => t.Archived == false));

            List<TicketHistory> ticketHistories = companyTickets.SelectMany(t => t.History).ToList();

            return ticketHistories;
           
        }
    }
}
