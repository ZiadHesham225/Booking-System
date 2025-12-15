using Booking_System.Infrastructure.Data;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Interfaces;
using Booking_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            return await dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}




