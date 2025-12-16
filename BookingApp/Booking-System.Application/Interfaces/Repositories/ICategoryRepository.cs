using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category> GetCategoryByNameAsync(string name);
    }
}


