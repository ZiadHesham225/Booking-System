using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
{
    public interface ITicketTypeRepository : IGenericRepository<TicketType>
    {
        Task<IEnumerable<TicketType>> GetActiveTicketTypesAsync();
        Task<TicketType> GetByNameAsync(string name);
    }
}


