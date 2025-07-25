using Booking_System.DTOs;

namespace Booking_System.Business_Logic.Interfaces
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
