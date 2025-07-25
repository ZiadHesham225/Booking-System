using Booking_System.Common;
using Booking_System.DTOs;
using Booking_System.Models;

namespace Booking_System.Data_Access.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<PaginatedResponse<Event>> GetAllEventsAsync(int pageIndex = 1, int pageSize = 20);
        Task<PaginatedResponse<Event>> SearchEventsAsync(EventSearchHandler searchHandler, int pageIndex = 1, int pageSize = 20);
        Task<Event> GetEventByIdAsync(int id);
        Task<Event> CreateEventAsync(Event eventEntity);
        Task<bool> IsAlreadyBooked(string userId, int eventId);
        Task<HashSet<int>> GetUserBookedEventIdsAsync(string userId);
        Task<PaginatedResponse<Event>> GetUpcomingEventsAsync(int pageIndex = 1, int pageSize = 20);
        Task<bool> HasAvailableTickets(int eventId, int ticketTypeId, int requestedSeats);
        void UpdateEvent(Event eventEntity);
        Task<int> CountAsync();
        Task<List<TopEventDto>> GetTopBookedEventsAsync(int take);
        Task DeleteEventAsync(int id);
        Task SaveChangesAsync();
    }
}
