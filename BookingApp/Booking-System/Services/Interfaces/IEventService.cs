using Booking_System.Common;
using Booking_System.DTOs;
using Booking_System.Models;

namespace Booking_System.Business_Logic.Interfaces
{
    public interface IEventService
    {
        Task<PaginatedResponse<EventDto>> GetAllEventsAsync(string? userId = null, int pageIndex = 1, int pageSize = 20);
        Task<PaginatedResponse<EventDto>> SearchEventsAsync(EventSearchHandler searchHandler, string? userId = null, int pageIndex = 1, int pageSize = 20);
        Task<EventDto> GetEventByIdAsync(int id, string? userId = null);
        Task<CreateEventDto> CreateEventAsync(CreateEventDto dto, List<CreateEventTicketTypeDto> EventTicketTypes);
        Task UpdateEventAsync(UpdateEventDto eventDto);
        Task DeleteEventAsync(int id);
    }
}
