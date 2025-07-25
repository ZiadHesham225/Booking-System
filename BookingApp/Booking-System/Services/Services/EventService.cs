using Booking_System.Common;
using Booking_System.Data;
using Booking_System.DTOs;
using Booking_System.Models;
using Booking_System.Business_Logic.Interfaces;

namespace Booking_System.Business_Logic.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageService _imageService;

        public EventService(IUnitOfWork unitOfWork, ImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }
        public async Task<CreateEventDto> CreateEventAsync(CreateEventDto dto, List<CreateEventTicketTypeDto> EventTicketTypes)
        {
            try
            {
                var newEvent = new Event
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.EndDateTime,
                    City = dto.City,
                    Address = dto.Address,
                    CategoryId = dto.CategoryId
                };

                if (dto.EventPicture != null)
                {
                    newEvent.ImageUrl = await _imageService.SaveImageAsync(dto.EventPicture, "Events");
                }

                var created = await _unitOfWork.Events.CreateEventAsync(newEvent);

                foreach (var ticketTypeDto in EventTicketTypes)
                {
                    var eventTicketType = new EventTicketType
                    {
                        Event = created,
                        TicketTypeId = ticketTypeDto.TicketTypeId,
                        Price = ticketTypeDto.Price,
                        TotalSeats = ticketTypeDto.TotalSeats,
                        AvailableSeats = ticketTypeDto.TotalSeats
                    };
                    await _unitOfWork.EventTicketTypes.CreateAsync(eventTicketType);
                }

                await _unitOfWork.CommitAsync();

                return new CreateEventDto
                {
                    Title = created.Title,
                    Description = created.Description,
                    StartDateTime = created.StartDateTime,
                    EndDateTime = created.EndDateTime,
                    City = created.City,
                    Address = created.Address,
                    CategoryId = created.CategoryId
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error creating event with ticket types", ex);
            }
        }

        public async Task<PaginatedResponse<EventDto>> GetAllEventsAsync(string? userId = null, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var result = await _unitOfWork.Events.GetAllEventsAsync(pageIndex, pageSize);
                var eventDtos = result.Items.Select(MapToEventDto).ToList();

                if (!string.IsNullOrEmpty(userId))
                {
                    var bookedEventIds = await _unitOfWork.Events.GetUserBookedEventIdsAsync(userId);
                    foreach (var eventDto in eventDtos)
                    {
                        eventDto.isBooked = bookedEventIds.Contains(eventDto.EventId);
                    }
                }

                return new PaginatedResponse<EventDto>
                {
                    Items = eventDtos,
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalItems,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving events", ex);
            }
        }

        public async Task<PaginatedResponse<EventDto>> GetUpcomingEventsAsync(string? userId = null, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var result = await _unitOfWork.Events.GetUpcomingEventsAsync(pageIndex, pageSize);
                var eventDtos = result.Items.Select(MapToEventDto).ToList();

                if (!string.IsNullOrEmpty(userId))
                {
                    var bookedEventIds = await _unitOfWork.Events.GetUserBookedEventIdsAsync(userId);
                    foreach (var eventDto in eventDtos)
                    {
                        eventDto.isBooked = bookedEventIds.Contains(eventDto.EventId);
                    }
                }

                return new PaginatedResponse<EventDto>
                {
                    Items = eventDtos,
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalItems,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving upcoming events", ex);
            }
        }

        public async Task<PaginatedResponse<EventDto>> SearchEventsAsync(EventSearchHandler searchHandler, string? userId = null, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var result = await _unitOfWork.Events.SearchEventsAsync(searchHandler, pageIndex, pageSize);
                var eventDtos = result.Items.Select(MapToEventDto).ToList();

                if (!string.IsNullOrEmpty(userId))
                {
                    var bookedEventIds = await _unitOfWork.Events.GetUserBookedEventIdsAsync(userId);
                    foreach (var eventDto in eventDtos)
                    {
                        eventDto.isBooked = bookedEventIds.Contains(eventDto.EventId);
                    }
                }

                return new PaginatedResponse<EventDto>
                {
                    Items = eventDtos,
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalItems,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error searching events", ex);
            }
        }

        public async Task<EventDto> GetEventByIdAsync(int id, string? userId = null)
        {
            try
            {
                var entity = await _unitOfWork.Events.GetEventByIdAsync(id);
                if (entity == null)
                    throw new ArgumentException("Event not found.");

                var eventDto = MapToEventDto(entity);

                if (!string.IsNullOrEmpty(userId))
                {
                    var bookedEventIds = await _unitOfWork.Events.GetUserBookedEventIdsAsync(userId);
                    eventDto.isBooked = bookedEventIds.Contains(eventDto.EventId);
                }

                return eventDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving event with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<EventTicketTypeDto>> GetEventTicketTypesAsync(int eventId)
        {
            try
            {
                var eventTicketTypes = await _unitOfWork.EventTicketTypes.GetByEventIdAsync(eventId);
                return eventTicketTypes.Select(MapToEventTicketTypeDto);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving ticket types for event {eventId}", ex);
            }
        }

        public async Task<bool> HasAvailableTicketsAsync(int eventId, int ticketTypeId, int requestedSeats)
        {
            try
            {
                return await _unitOfWork.Events.HasAvailableTickets(eventId, ticketTypeId, requestedSeats);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error checking ticket availability for event {eventId}", ex);
            }
        }

        public async Task UpdateEventAsync(UpdateEventDto dto)
        {
            try
            {
                var existingEvent = await _unitOfWork.Events.GetEventByIdAsync(dto.EventId);
                if (existingEvent == null)
                    throw new ArgumentException("Event not found.");

                existingEvent.Title = dto.Title;
                existingEvent.Description = dto.Description;
                existingEvent.StartDateTime = dto.StartDateTime;
                existingEvent.EndDateTime = dto.EndDateTime;
                existingEvent.City = dto.City;
                existingEvent.Address = dto.Address;
                existingEvent.CategoryId = dto.CategoryId;

                if (dto.EventPicture != null)
                {
                    if (!string.IsNullOrEmpty(existingEvent.ImageUrl))
                        _imageService.DeleteImage(existingEvent.ImageUrl);
                    existingEvent.ImageUrl = await _imageService.SaveImageAsync(dto.EventPicture, "Events");
                }

                _unitOfWork.Events.UpdateEvent(existingEvent);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating event with ID {dto.EventId}", ex);
            }
        }

        public async Task UpdateEventTicketTypesAsync(int eventId, List<UpdateEventTicketTypeDto> ticketTypes)
        {
            try
            {
                var existingEvent = await _unitOfWork.Events.GetEventByIdAsync(eventId);
                if (existingEvent == null)
                    throw new ArgumentException("Event not found.");

                foreach (var ticketTypeDto in ticketTypes)
                {
                    var eventTicketType = await _unitOfWork.EventTicketTypes.GetByEventAndTicketTypeAsync(eventId, ticketTypeDto.TicketTypeId);
                    if (eventTicketType != null)
                    {
                        eventTicketType.Price = ticketTypeDto.Price;
                        eventTicketType.TotalSeats = ticketTypeDto.TotalSeats;
                        var seatDifference = ticketTypeDto.TotalSeats - eventTicketType.TotalSeats;
                        eventTicketType.AvailableSeats = Math.Max(0, eventTicketType.AvailableSeats + seatDifference);

                        _unitOfWork.EventTicketTypes.Update(eventTicketType);
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating ticket types for event {eventId}", ex);
            }
        }

        public async Task DeleteEventAsync(int id)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetByEventIdAsync(id);
                foreach (var booking in bookings)
                {
                    await _unitOfWork.Bookings.DeleteAsync(booking.BookingId);
                }
                var eventTicketTypes = await _unitOfWork.EventTicketTypes.GetByEventIdAsync(id);
                foreach (var eventTicketType in eventTicketTypes)
                {
                    await _unitOfWork.EventTicketTypes.DeleteAsync(eventTicketType.Id);
                }
                await _unitOfWork.Events.DeleteEventAsync(id);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting event with ID {id}", ex);
            }
        }

        public async Task<bool> IsEventBookedByUserAsync(string userId, int eventId)
        {
            try
            {
                return await _unitOfWork.Events.IsAlreadyBooked(userId, eventId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error checking if event {eventId} is booked by user", ex);
            }
        }

        private EventDto MapToEventDto(Event eventEntity)
        {
            return new EventDto
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                StartDateTime = eventEntity.StartDateTime,
                EndDateTime = eventEntity.EndDateTime,
                City = eventEntity.City,
                Address = eventEntity.Address,
                CategoryName = eventEntity.Category?.Name,
                CategoryId = eventEntity.CategoryId,
                ImageUrl = eventEntity.ImageUrl,
                EventTicketTypes = eventEntity.EventTicketTypes?.Select(MapToEventTicketTypeDto).ToList() ?? new List<EventTicketTypeDto>()
            };
        }

        private EventTicketTypeDto MapToEventTicketTypeDto(EventTicketType eventTicketType)
        {
            return new EventTicketTypeDto
            {
                Id = eventTicketType.Id,
                EventId = eventTicketType.EventId,
                TicketTypeId = eventTicketType.TicketTypeId,
                TicketTypeName = eventTicketType.TicketType?.Name,
                Price = eventTicketType.Price,
                TotalSeats = eventTicketType.TotalSeats,
                AvailableSeats = eventTicketType.AvailableSeats
            };
        }
    }
}