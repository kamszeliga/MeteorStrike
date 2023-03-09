using MeteorStrike.Models;

namespace MeteorStrike.Services.Interfaces
{
    public interface IBTTicketService 
    {
        public Task AddTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketAsync(int? ticketId);

        public Task<IEnumerable<Ticket>> GetTicketsAsync(int projectId);

        public Task ArchiveTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);


		//-------------------------------------------------

		public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

	}
}
