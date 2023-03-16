using MeteorStrike.Models;

namespace MeteorStrike.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketAsync(int? ticketId);

        public Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId);

        public Task ArchiveTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);


        //-------------------------------------------------

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<IEnumerable<TicketPriority>> GetTicketPriorities();

        public Task<IEnumerable<TicketStatus>> GetTicketStatuses();

        public Task<IEnumerable<TicketType>> GetTicketTypes();

        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId);

        public Task AddTicketComment(TicketComment ticketComment);

        public Task<bool> TicketExists(int id);

        public Task<IEnumerable<Ticket>> GetUserTicketsAsync(string? userId, int? companyId);

		public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        public Task<List<TicketHistory>> GetUserTicketHistoryAsync(int? companyId);


    }
}
