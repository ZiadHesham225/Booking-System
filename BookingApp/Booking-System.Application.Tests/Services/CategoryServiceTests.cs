using Booking_System.Application.DTOs.Category;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Services;
using Booking_System.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Booking_System.Application.Tests.Services
{
    /// <summary>
    /// Unit tests for CategoryService covering CRUD operations.
    /// </summary>
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();

            _mockUnitOfWork.Setup(x => x.Categories).Returns(_mockCategoryRepository.Object);

            _sut = new CategoryService(_mockUnitOfWork.Object);
        }

        #region GetAllAsync Tests

        /// <summary>
        /// Verifies that GetAllAsync returns all categories.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_HasCategories_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, Name = "Music" },
                new Category { CategoryId = 2, Name = "Sports" },
                new Category { CategoryId = 3, Name = "Arts" }
            };

            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Name == "Music");
            result.Should().Contain(c => c.Name == "Sports");
            result.Should().Contain(c => c.Name == "Arts");
        }

        /// <summary>
        /// Verifies that GetAllAsync returns empty collection when no categories exist.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_NoCategories_ReturnsEmptyCollection()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        /// <summary>
        /// Verifies that GetByIdAsync returns category for valid ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { CategoryId = categoryId, Name = "Music" };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be("Music");
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when category is not found.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var categoryId = 999;

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.GetByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync Tests

        /// <summary>
        /// Verifies that CreateAsync successfully creates a new category.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ValidData_ReturnsCreatedCategory()
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = "New Category" };

            _mockCategoryRepository.Setup(x => x.CreateAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) =>
                {
                    c.CategoryId = 1;
                    return c;
                });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Category");
            _mockCategoryRepository.Verify(x => x.CreateAsync(It.Is<Category>(c => c.Name == createDto.Name)), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that CreateAsync handles various category names.
        /// </summary>
        [Theory]
        [InlineData("Music")]
        [InlineData("Sports & Recreation")]
        [InlineData("Art & Culture")]
        [InlineData("Technology")]
        public async Task CreateAsync_VariousNames_CreatesSuccessfully(string categoryName)
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = categoryName };

            _mockCategoryRepository.Setup(x => x.CreateAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) =>
                {
                    c.CategoryId = 1;
                    return c;
                });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateAsync(createDto);

            // Assert
            result.Name.Should().Be(categoryName);
        }

        #endregion

        #region UpdateAsync Tests

        /// <summary>
        /// Verifies that UpdateAsync successfully updates an existing category.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ValidData_UpdatesCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 1, Name = "Updated Music" };
            var existingCategory = new Category { CategoryId = 1, Name = "Music" };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryDto.Id))
                .ReturnsAsync(existingCategory);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateAsync(categoryDto);

            // Assert
            existingCategory.Name.Should().Be("Updated Music");
            _mockCategoryRepository.Verify(x => x.Update(existingCategory), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that UpdateAsync throws exception when category is not found.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_CategoryNotFound_ThrowsArgumentException()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 999, Name = "Non-existent" };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryDto.Id))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(categoryDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Category not found");
        }

        #endregion

        #region DeleteAsync Tests

        /// <summary>
        /// Verifies that DeleteAsync successfully deletes an existing category.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ValidId_DeletesCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { CategoryId = categoryId, Name = "To Delete" };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);
            _mockCategoryRepository.Setup(x => x.DeleteAsync(categoryId))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteAsync(categoryId);

            // Assert
            _mockCategoryRepository.Verify(x => x.DeleteAsync(categoryId), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteAsync throws exception when category is not found.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_CategoryNotFound_ThrowsArgumentException()
        {
            // Arrange
            var categoryId = 999;

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(categoryId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Category not found");
        }

        #endregion
    }
}
