using Booking_System.Models;

namespace Booking_System.Data_Access.Interfaces
{
    public interface ITicketTypeRepository : IGenericRepository<TicketType>
    {
        Task<IEnumerable<TicketType>> GetActiveTicketTypesAsync();
        Task<TicketType> GetByNameAsync(string name);
    }
}
