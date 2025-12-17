using Booking_System.Application.Common;
using Booking_System.Application.Interfaces;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using Booking_System.Application.DTOs.Event;
using Booking_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Infrastructure.Repositories
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Event> CreateEventAsync(Event eventEntity)
        {
            return await CreateAsync(eventEntity);
        }

        public async Task DeleteEventAsync(int id)
        {
            await DeleteAsync(id);
        }

        public async Task<PaginatedResponse<Event>> GetAllEventsAsync(int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.EventTicketTypes)
                    .ThenInclude(ett => ett.TicketType);
            var pagedList = await PaginatedList<Event>.CreateAsync(query, pageIndex, pageSize);

            return new PaginatedResponse<Event>
            {
                Items = pagedList,
                CurrentPage = pagedList.PageIndex,
                TotalPages = pagedList.TotalPages,
                TotalItems = pagedList.TotalCount,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage
            };
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.EventTicketTypes)
                    .ThenInclude(ett => ett.TicketType)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }

        public async Task<PaginatedResponse<Event>> GetUpcomingEventsAsync(int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.EventTicketTypes)
                    .ThenInclude(ett => ett.TicketType)
                .Where(e => e.StartDateTime > DateTime.UtcNow);
            var pagedList = await PaginatedList<Event>.CreateAsync(query, pageIndex, pageSize);
            return new PaginatedResponse<Event>
            {
                Items = pagedList,
                CurrentPage = pagedList.PageIndex,
                TotalPages = pagedList.TotalPages,
                TotalItems = pagedList.TotalCount,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage
            };
        }

        public async Task SaveChangesAsync()
        {
            await SaveAsync();
        }

        public async Task<PaginatedResponse<Event>> SearchEventsAsync(EventSearchHandler searchHandler, int pageIndex = 1, int pageSize = 20)
        {
            IQueryable<Event> events = dbSet
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.EventTicketTypes)
                    .ThenInclude(ett => ett.TicketType);

            if (!string.IsNullOrEmpty(searchHandler.Keyword))
            {
                events = events.Where(e => e.Title.Contains(searchHandler.Keyword) || e.Description.Contains(searchHandler.Keyword));
            }
            if (searchHandler.StartDate.HasValue)
            {
                events = events.Where(e => e.StartDateTime >= searchHandler.StartDate.Value);
            }
            if (searchHandler.CategoryId.HasValue)
            {
                events = events.Where(e => e.CategoryId == searchHandler.CategoryId.Value);
            }
            if (!string.IsNullOrEmpty(searchHandler.City))
            {
                events = events.Where(e => e.City.Contains(searchHandler.City));
            }
            var pagedList = await PaginatedList<Event>.CreateAsync(events, pageIndex, pageSize);

            return new PaginatedResponse<Event>
            {
                Items = pagedList,
                CurrentPage = pagedList.PageIndex,
                TotalPages = pagedList.TotalPages,
                TotalItems = pagedList.TotalCount,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage
            };
        }
        public async Task<bool> IsAlreadyBooked(string userId, int eventId)
        {
            return await _context.Bookings.AnyAsync(booking => booking.UserId == userId && booking.EventId == eventId);
        }

        public async Task<HashSet<int>> GetUserBookedEventIdsAsync(string userId)
        {
            return (await _context.Bookings
                .Where(booking => booking.UserId == userId)
                .Select(booking => booking.EventId)
                .ToListAsync())
                .ToHashSet();
        }
        public async Task<bool> HasAvailableTickets(int eventId, int ticketTypeId, int requestedSeats)
        {
            var eventTicketType = await _context.EventTicketTypes
                .FirstOrDefaultAsync(ett => ett.EventId == eventId && ett.TicketTypeId == ticketTypeId);

            return eventTicketType != null && eventTicketType.AvailableSeats >= requestedSeats;
        }

        public void UpdateEvent(Event eventEntity)
        {
            Update(eventEntity);
        }
        public Task<int> CountAsync()
        {
            return _context.Events.CountAsync();
        }

        public async Task<List<TopEventDto>> GetTopBookedEventsAsync(int take)
        {
            // Optimized query to avoid N+1 problem with SelectMany
            return await _context.Bookings
                .AsNoTracking()
                .GroupBy(b => new { b.EventId, b.Event.Title })
                .Select(g => new TopEventDto
                {
                    EventId = g.Key.EventId,
                    Title = g.Key.Title,
                    TotalBookings = g.Count()
                })
                .OrderByDescending(e => e.TotalBookings)
                .Take(take)
                .ToListAsync();
        }
    }
}





