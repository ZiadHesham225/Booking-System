using Booking_System.Models;

namespace Booking_System.Data_Access.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category> GetCategoryByNameAsync(string name);
    }
}
