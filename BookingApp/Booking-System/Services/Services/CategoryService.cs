using Booking_System.Business_Logic.Interfaces;
using Booking_System.Data;
using Booking_System.DTOs;
using Booking_System.Models;

namespace Booking_System.Business_Logic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories.Select(c => new CategoryDto { Id = c.CategoryId, Name = c.Name });
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDto { Id = category.CategoryId, Name = category.Name };
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            var category = new Category { Name = dto.Name };
            await _unitOfWork.Categories.CreateAsync(category);
            await _unitOfWork.CommitAsync();
            return new CategoryDto { Id = category.CategoryId, Name = category.Name };
        }

        public async Task UpdateAsync(CategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.Id);
            if (category == null)
                throw new ArgumentException("Category not found");

            category.Name = dto.Name;
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Category not found");

            await _unitOfWork.Categories.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }
    }
}
