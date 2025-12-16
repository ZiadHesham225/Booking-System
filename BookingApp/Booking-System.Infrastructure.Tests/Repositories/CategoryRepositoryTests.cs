using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for CategoryRepository using EF Core InMemory database.
    /// Tests category-specific operations including lookup by name.
    /// </summary>
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CategoryRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetCategoryByNameAsync Tests

        /// <summary>
        /// Tests that GetCategoryByNameAsync returns category when name exists.
        /// </summary>
        [Fact]
        public async Task GetCategoryByNameAsync_WithExistingName_ShouldReturnCategory()
        {
            // Arrange
            var category = new Category
            {
                Name = "Concerts"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCategoryByNameAsync("Concerts");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Concerts");
        }

        /// <summary>
        /// Tests that GetCategoryByNameAsync returns null when name doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetCategoryByNameAsync_WithNonExistingName_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetCategoryByNameAsync("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetCategoryByNameAsync is case-sensitive in InMemory provider.
        /// </summary>
        [Fact]
        public async Task GetCategoryByNameAsync_IsCaseSensitive()
        {
            // Arrange
            var category = new Category
            {
                Name = "Concerts"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCategoryByNameAsync("concerts");

            // Assert
            result.Should().BeNull(); // Case-sensitive in InMemory provider
        }

        /// <summary>
        /// Tests that GetCategoryByNameAsync returns correct category when multiple exist.
        /// </summary>
        [Fact]
        public async Task GetCategoryByNameAsync_WithMultipleCategories_ShouldReturnCorrectOne()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Concerts" },
                new Category { Name = "Sports" },
                new Category { Name = "Theater" }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCategoryByNameAsync("Sports");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Sports");
        }

        #endregion

        #region CRUD Operations Tests

        /// <summary>
        /// Tests creating a new category.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithValidCategory_ShouldCreateCategory()
        {
            // Arrange
            var category = new Category
            {
                Name = "New Category"
            };

            // Act
            var result = await _repository.CreateAsync(category);
            await _context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().BeGreaterThan(0);
            _context.Categories.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests getting all categories.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithMultipleCategories_ShouldReturnAll()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Category 1" },
                new Category { Name = "Category 2" },
                new Category { Name = "Category 3" }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        /// <summary>
        /// Tests getting category by ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnCategory()
        {
            // Arrange
            var category = new Category
            {
                Name = "Test Category"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(category.CategoryId);

            // Assert
            result.Should().NotBeNull();
            ((Category)result!).Name.Should().Be("Test Category");
        }

        /// <summary>
        /// Tests updating an existing category.
        /// </summary>
        [Fact]
        public async Task Update_WithExistingCategory_ShouldModifyCategory()
        {
            // Arrange
            var category = new Category
            {
                Name = "Original"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            _context.Entry(category).State = EntityState.Detached;

            // Act
            category.Name = "Updated";
            _repository.Update(category);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(category.CategoryId);
            updatedCategory!.Name.Should().Be("Updated");
        }

        /// <summary>
        /// Tests deleting a category.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithExistingCategory_ShouldRemoveCategory()
        {
            // Arrange
            var category = new Category
            {
                Name = "To Delete"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            var categoryId = category.CategoryId;

            // Act
            await _repository.DeleteAsync(categoryId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedCategory = await _context.Categories.FindAsync(categoryId);
            deletedCategory.Should().BeNull();
        }

        #endregion

        #region Edge Cases Tests

        /// <summary>
        /// Tests that empty string search returns null.
        /// </summary>
        [Fact]
        public async Task GetCategoryByNameAsync_WithEmptyString_ShouldReturnNull()
        {
            // Arrange
            var category = new Category
            {
                Name = "Test"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCategoryByNameAsync("");

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests getting all categories when database is empty.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithNoCategories_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that partial name match doesn't return results.
        /// </summary>
        [Fact]
        public async Task GetCategoryByNameAsync_WithPartialMatch_ShouldReturnNull()
        {
            // Arrange
            var category = new Category
            {
                Name = "Concert Music"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCategoryByNameAsync("Concert");

            // Assert
            result.Should().BeNull(); // Exact match only
        }

        #endregion
    }
}
