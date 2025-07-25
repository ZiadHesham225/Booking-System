using Booking_System.Data;
using Booking_System.Data_Access.Interfaces;
using Booking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Data_Access.Repositories
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
