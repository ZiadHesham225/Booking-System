using Booking_System.Data;
using Booking_System.Data_Access.Interfaces;
using Booking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Data_Access.Repositories
{
    public class EventTicketTypeRepository : GenericRepository<EventTicketType>, IEventTicketTypeRepository
    {
        public EventTicketTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EventTicketType>> GetByEventIdAsync(int eventId)
        {
            return await dbSet
                .Include(ett => ett.TicketType)
                .Where(ett => ett.EventId == eventId)
                .ToListAsync();
        }

        public async Task<EventTicketType> GetByEventAndTicketTypeAsync(int eventId, int ticketTypeId)
        {
            return await dbSet
                .Include(ett => ett.TicketType)
                .FirstOrDefaultAsync(ett => ett.EventId == eventId && ett.TicketTypeId == ticketTypeId);
        }

        public async Task<bool> HasAvailableSeatsAsync(int eventTicketTypeId, int requestedSeats)
        {
            var eventTicketType = await dbSet.FindAsync(eventTicketTypeId);
            return eventTicketType != null && eventTicketType.AvailableSeats >= requestedSeats;
        }

        public async Task UpdateAvailableSeatsAsync(int eventTicketTypeId, int seatChange)
        {
            var eventTicketType = await dbSet.FindAsync(eventTicketTypeId);
            if (eventTicketType != null)
            {
                eventTicketType.AvailableSeats += seatChange;
                if (eventTicketType.AvailableSeats < 0)
                    eventTicketType.AvailableSeats = 0;
                if (eventTicketType.AvailableSeats > eventTicketType.TotalSeats)
                    eventTicketType.AvailableSeats = eventTicketType.TotalSeats;
            }
        }
    }
}
