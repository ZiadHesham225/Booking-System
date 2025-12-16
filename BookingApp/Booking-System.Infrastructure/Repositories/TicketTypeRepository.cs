using Booking_System.Application.Interfaces;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using Booking_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Infrastructure.Repositories
{
    public class TicketTypeRepository : GenericRepository<TicketType>, ITicketTypeRepository
    {
        public TicketTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TicketType>> GetActiveTicketTypesAsync()
        {
            return await dbSet
                .AsNoTracking()
                .Where(tt => tt.IsActive)
                .ToListAsync();
        }

        public async Task<TicketType?> GetByNameAsync(string name)
        {
            return await dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(tt => tt.Name == name);
        }
    }
}




