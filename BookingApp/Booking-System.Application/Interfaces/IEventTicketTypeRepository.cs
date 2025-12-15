using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
{
    public interface IEventTicketTypeRepository : IGenericRepository<EventTicketType>
    {
        Task<IEnumerable<EventTicketType>> GetByEventIdAsync(int eventId);
        Task<EventTicketType> GetByEventAndTicketTypeAsync(int eventId, int ticketTypeId);
        Task<bool> HasAvailableSeatsAsync(int eventTicketTypeId, int requestedSeats);
        Task UpdateAvailableSeatsAsync(int eventTicketTypeId, int seatChange);
    }
}


