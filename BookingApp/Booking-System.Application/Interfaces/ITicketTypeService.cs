using Booking_System.Application.DTOs.TicketType;
using Booking_System.Application.DTOs.EventTicketType;

namespace Booking_System.Application.Interfaces
{
    public interface ITicketTypeService
    {
        Task<IEnumerable<TicketTypeDto>> GetAllAsync();
        Task<IEnumerable<TicketTypeDto>> GetActiveTicketTypesAsync();
        Task<TicketTypeDto> GetByIdAsync(int id);
        Task<TicketTypeDto> GetByNameAsync(string name);
        Task<TicketTypeDto> CreateAsync(CreateTicketTypeDto dto);
        Task UpdateAsync(UpdateTicketTypeDto dto);
        Task DeleteAsync(int id);
        Task ToggleActiveStatusAsync(int id);
    }
}



