using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Event;
using Booking_System.Application.DTOs.EventTicketType;
using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
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



