using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for TicketTypeRepository using EF Core InMemory database.
    /// Tests ticket type-specific operations including active filtering and name lookup.
    /// </summary>
    public class TicketTypeRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TicketTypeRepository _repository;

        public TicketTypeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TicketTypeRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetActiveTicketTypesAsync Tests

        /// <summary>
        /// Tests that GetActiveTicketTypesAsync returns only active ticket types.
        /// </summary>
        [Fact]
        public async Task GetActiveTicketTypesAsync_ShouldReturnOnlyActiveTicketTypes()
        {
            // Arrange
            var ticketTypes = new List<TicketType>
            {
                new TicketType { Name = "General Admission", IsActive = true },
                new TicketType { Name = "VIP", IsActive = true },
                new TicketType { Name = "Disabled", IsActive = false }
            };
            await _context.TicketTypes.AddRangeAsync(ticketTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveTicketTypesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(tt => tt.IsActive).Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetActiveTicketTypesAsync returns empty when no active ticket types exist.
        /// </summary>
        [Fact]
        public async Task GetActiveTicketTypesAsync_WithNoActiveTypes_ShouldReturnEmpty()
        {
            // Arrange
            var ticketTypes = new List<TicketType>
            {
                new TicketType { Name = "Inactive 1", IsActive = false },
                new TicketType { Name = "Inactive 2", IsActive = false }
            };
            await _context.TicketTypes.AddRangeAsync(ticketTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveTicketTypesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetActiveTicketTypesAsync returns all when all are active.
        /// </summary>
        [Fact]
        public async Task GetActiveTicketTypesAsync_WithAllActive_ShouldReturnAll()
        {
            // Arrange
            var ticketTypes = new List<TicketType>
            {
                new TicketType { Name = "General", IsActive = true },
                new TicketType { Name = "VIP", IsActive = true },
                new TicketType { Name = "Premium", IsActive = true }
            };
            await _context.TicketTypes.AddRangeAsync(ticketTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveTicketTypesAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        #endregion

        #region GetByNameAsync Tests

        /// <summary>
        /// Tests that GetByNameAsync returns ticket type when name exists.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_WithExistingName_ShouldReturnTicketType()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "VIP",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByNameAsync("VIP");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("VIP");
        }

        /// <summary>
        /// Tests that GetByNameAsync returns null when name doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_WithNonExistingName_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByNameAsync("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetByNameAsync is case-sensitive.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_IsCaseSensitive()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "VIP",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByNameAsync("vip");

            // Assert
            result.Should().BeNull(); // Case-sensitive in InMemory provider
        }

        /// <summary>
        /// Tests that GetByNameAsync returns inactive ticket types as well.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_ShouldReturnInactiveTicketTypes()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "Disabled Type",
                IsActive = false
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByNameAsync("Disabled Type");

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeFalse();
        }

        #endregion

        #region CRUD Operations Tests

        /// <summary>
        /// Tests creating a new ticket type.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithValidTicketType_ShouldCreateTicketType()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "Premium",
                IsActive = true
            };

            // Act
            var result = await _repository.CreateAsync(ticketType);
            await _context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.TicketTypeId.Should().BeGreaterThan(0);
            _context.TicketTypes.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests getting all ticket types.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithMultipleTicketTypes_ShouldReturnAll()
        {
            // Arrange
            var ticketTypes = new List<TicketType>
            {
                new TicketType { Name = "General", IsActive = true },
                new TicketType { Name = "VIP", IsActive = true },
                new TicketType { Name = "Premium", IsActive = false }
            };
            await _context.TicketTypes.AddRangeAsync(ticketTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3); // Returns all regardless of IsActive
        }

        /// <summary>
        /// Tests getting ticket type by ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnTicketType()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "Test Type",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(ticketType.TicketTypeId);

            // Assert
            result.Should().NotBeNull();
            ((TicketType)result!).Name.Should().Be("Test Type");
        }

        /// <summary>
        /// Tests updating an existing ticket type.
        /// </summary>
        [Fact]
        public async Task Update_WithExistingTicketType_ShouldModifyTicketType()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "Original",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();
            _context.Entry(ticketType).State = EntityState.Detached;

            // Act
            ticketType.Name = "Updated";
            ticketType.IsActive = false;
            _repository.Update(ticketType);
            await _context.SaveChangesAsync();

            // Assert
            var updatedType = await _context.TicketTypes.FindAsync(ticketType.TicketTypeId);
            updatedType!.Name.Should().Be("Updated");
            updatedType.IsActive.Should().BeFalse();
        }

        /// <summary>
        /// Tests toggling IsActive status.
        /// </summary>
        [Fact]
        public async Task Update_ToggleActiveStatus_ShouldChangeIsActive()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "Toggle Type",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();
            _context.Entry(ticketType).State = EntityState.Detached;

            // Act - Toggle to false
            ticketType.IsActive = false;
            _repository.Update(ticketType);
            await _context.SaveChangesAsync();

            // Assert
            var updatedType = await _context.TicketTypes.FindAsync(ticketType.TicketTypeId);
            updatedType!.IsActive.Should().BeFalse();

            // Act - Toggle back to true
            _context.Entry(updatedType).State = EntityState.Detached;
            updatedType.IsActive = true;
            _repository.Update(updatedType);
            await _context.SaveChangesAsync();

            // Assert
            var finalType = await _context.TicketTypes.FindAsync(ticketType.TicketTypeId);
            finalType!.IsActive.Should().BeTrue();
        }

        /// <summary>
        /// Tests deleting a ticket type.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithExistingTicketType_ShouldRemoveTicketType()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "To Delete",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();
            var typeId = ticketType.TicketTypeId;

            // Act
            await _repository.DeleteAsync(typeId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedType = await _context.TicketTypes.FindAsync(typeId);
            deletedType.Should().BeNull();
        }

        #endregion

        #region Edge Cases Tests

        /// <summary>
        /// Tests getting active ticket types when none exist.
        /// </summary>
        [Fact]
        public async Task GetActiveTicketTypesAsync_WithNoTicketTypes_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetActiveTicketTypesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetByNameAsync with empty string returns null.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_WithEmptyString_ShouldReturnNull()
        {
            // Arrange
            var ticketType = new TicketType { Name = "Test", IsActive = true };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByNameAsync("");

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that partial name match doesn't return results.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_WithPartialMatch_ShouldReturnNull()
        {
            // Arrange
            var ticketType = new TicketType
            {
                Name = "General Admission",
                IsActive = true
            };
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByNameAsync("General");

            // Assert
            result.Should().BeNull(); // Exact match only
        }

        #endregion
    }
}
