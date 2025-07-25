using Booking_System.Data;
using Booking_System.Data_Access.Repositories;
using Booking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Data_Access.Interfaces
{
    public class TicketTypeRepository : GenericRepository<TicketType>, ITicketTypeRepository
    {
        public TicketTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TicketType>> GetActiveTicketTypesAsync()
        {
            return await dbSet.Where(tt => tt.IsActive).ToListAsync();
        }

        public async Task<TicketType> GetByNameAsync(string name)
        {
            return await dbSet.FirstOrDefaultAsync(tt => tt.Name == name);
        }
    }
}
