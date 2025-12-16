using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for GenericRepository using EF Core InMemory database.
    /// Tests basic CRUD operations that all repositories inherit.
    /// </summary>
    public class GenericRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<Category> _repository;

        public GenericRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GenericRepository<Category>(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreateAsync Tests

        /// <summary>
        /// Tests that CreateAsync successfully adds a new entity to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithValidEntity_ShouldAddEntityToDatabase()
        {
            // Arrange
            var category = new Category
            {
                Name = "Test Category"
            };

            // Act
            var result = await _repository.CreateAsync(category);
            await _context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Category");
            _context.Categories.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests that CreateAsync returns the created entity with generated ID.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldReturnEntityWithGeneratedId()
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
            result.CategoryId.Should().BeGreaterThan(0);
        }

        #endregion

        #region GetByIdAsync Tests

        /// <summary>
        /// Tests that GetByIdAsync returns the correct entity when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnEntity()
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
            result!.Name.Should().Be("Test Category");
        }

        /// <summary>
        /// Tests that GetByIdAsync returns null when entity doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllAsync Tests

        /// <summary>
        /// Tests that GetAllAsync returns all entities in the database.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithMultipleEntities_ShouldReturnAllEntities()
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
        /// Tests that GetAllAsync returns empty collection when no entities exist.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithNoEntities_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Update Tests

        /// <summary>
        /// Tests that Update modifies an existing entity correctly.
        /// </summary>
        [Fact]
        public async Task Update_WithExistingEntity_ShouldModifyEntity()
        {
            // Arrange
            var category = new Category
            {
                Name = "Original Name"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            _context.Entry(category).State = EntityState.Detached;

            // Act
            category.Name = "Updated Name";
            _repository.Update(category);
            await _context.SaveChangesAsync();

            // Assert
            var updatedEntity = await _context.Categories.FindAsync(category.CategoryId);
            updatedEntity!.Name.Should().Be("Updated Name");
        }

        #endregion

        #region DeleteAsync Tests

        /// <summary>
        /// Tests that DeleteAsync removes an existing entity from the database.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithExistingId_ShouldRemoveEntity()
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
            var deletedEntity = await _context.Categories.FindAsync(categoryId);
            deletedEntity.Should().BeNull();
        }

        /// <summary>
        /// Tests that DeleteAsync handles non-existing entity gracefully.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ShouldNotThrowException()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var act = async () =>
            {
                await _repository.DeleteAsync(nonExistingId);
                await _context.SaveChangesAsync();
            };

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region SaveAsync Tests

        /// <summary>
        /// Tests that SaveAsync persists pending changes to the database.
        /// </summary>
        [Fact]
        public async Task SaveAsync_WithPendingChanges_ShouldPersistChanges()
        {
            // Arrange
            var category = new Category
            {
                Name = "Test Category"
            };
            await _repository.CreateAsync(category);

            // Act
            await _repository.SaveAsync();

            // Assert
            var savedEntity = await _context.Categories.FirstOrDefaultAsync();
            savedEntity.Should().NotBeNull();
            savedEntity!.Name.Should().Be("Test Category");
        }

        #endregion
    }
}
