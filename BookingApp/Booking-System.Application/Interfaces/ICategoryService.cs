using Booking_System.Application.DTOs.Category;

namespace Booking_System.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task UpdateAsync(CategoryDto dto);
        Task DeleteAsync(int id);
    }
}



