using Booking_System.Application.DTOs.TicketType;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Services;
using Booking_System.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Booking_System.Application.Tests.Services
{
    /// <summary>
    /// Unit tests for TicketTypeService covering CRUD operations and status management.
    /// </summary>
    public class TicketTypeServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITicketTypeRepository> _mockTicketTypeRepository;
        private readonly TicketTypeService _sut;

        public TicketTypeServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTicketTypeRepository = new Mock<ITicketTypeRepository>();

            _mockUnitOfWork.Setup(x => x.TicketTypes).Returns(_mockTicketTypeRepository.Object);

            _sut = new TicketTypeService(_mockUnitOfWork.Object);
        }

        #region GetAllAsync Tests

        /// <summary>
        /// Verifies that GetAllAsync returns all ticket types.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_HasTicketTypes_ReturnsAllTicketTypes()
        {
            // Arrange
            var ticketTypes = new List<TicketType>
            {
                new TicketType { TicketTypeId = 1, Name = "VIP", IsActive = true },
                new TicketType { TicketTypeId = 2, Name = "Regular", IsActive = true },
                new TicketType { TicketTypeId = 3, Name = "Student", IsActive = false }
            };

            _mockTicketTypeRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(ticketTypes);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(t => t.Name == "VIP");
            result.Should().Contain(t => t.Name == "Regular");
            result.Should().Contain(t => t.Name == "Student");
        }

        /// <summary>
        /// Verifies that GetAllAsync returns empty collection when no ticket types exist.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_NoTicketTypes_ReturnsEmptyCollection()
        {
            // Arrange
            _mockTicketTypeRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<TicketType>());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetActiveTicketTypesAsync Tests

        /// <summary>
        /// Verifies that GetActiveTicketTypesAsync returns only active ticket types.
        /// </summary>
        [Fact]
        public async Task GetActiveTicketTypesAsync_HasActiveTicketTypes_ReturnsActiveOnly()
        {
            // Arrange
            var activeTicketTypes = new List<TicketType>
            {
                new TicketType { TicketTypeId = 1, Name = "VIP", IsActive = true },
                new TicketType { TicketTypeId = 2, Name = "Regular", IsActive = true }
            };

            _mockTicketTypeRepository.Setup(x => x.GetActiveTicketTypesAsync())
                .ReturnsAsync(activeTicketTypes);

            // Act
            var result = await _sut.GetActiveTicketTypesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.IsActive);
        }

        /// <summary>
        /// Verifies that GetActiveTicketTypesAsync returns empty when no active ticket types exist.
        /// </summary>
        [Fact]
        public async Task GetActiveTicketTypesAsync_NoActiveTicketTypes_ReturnsEmptyCollection()
        {
            // Arrange
            _mockTicketTypeRepository.Setup(x => x.GetActiveTicketTypesAsync())
                .ReturnsAsync(new List<TicketType>());

            // Act
            var result = await _sut.GetActiveTicketTypesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        /// <summary>
        /// Verifies that GetByIdAsync returns ticket type for valid ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsTicketType()
        {
            // Arrange
            var ticketTypeId = 1;
            var ticketType = new TicketType { TicketTypeId = ticketTypeId, Name = "VIP", IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync(ticketType);

            // Act
            var result = await _sut.GetByIdAsync(ticketTypeId);

            // Assert
            result.Should().NotBeNull();
            result.TicketTypeId.Should().Be(ticketTypeId);
            result.Name.Should().Be("VIP");
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when ticket type is not found.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var ticketTypeId = 999;

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync((TicketType?)null);

            // Act
            var result = await _sut.GetByIdAsync(ticketTypeId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByNameAsync Tests

        /// <summary>
        /// Verifies that GetByNameAsync returns ticket type for valid name.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_ValidName_ReturnsTicketType()
        {
            // Arrange
            var ticketTypeName = "VIP";
            var ticketType = new TicketType { TicketTypeId = 1, Name = ticketTypeName, IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(ticketTypeName))
                .ReturnsAsync(ticketType);

            // Act
            var result = await _sut.GetByNameAsync(ticketTypeName);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(ticketTypeName);
        }

        /// <summary>
        /// Verifies that GetByNameAsync returns null when ticket type name is not found.
        /// </summary>
        [Fact]
        public async Task GetByNameAsync_InvalidName_ReturnsNull()
        {
            // Arrange
            var ticketTypeName = "NonExistent";

            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(ticketTypeName))
                .ReturnsAsync((TicketType?)null);

            // Act
            var result = await _sut.GetByNameAsync(ticketTypeName);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync Tests

        /// <summary>
        /// Verifies that CreateAsync successfully creates a new ticket type.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ValidData_ReturnsCreatedTicketType()
        {
            // Arrange
            var createDto = new CreateTicketTypeDto { Name = "Premium", IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(createDto.Name))
                .ReturnsAsync((TicketType?)null);
            _mockTicketTypeRepository.Setup(x => x.CreateAsync(It.IsAny<TicketType>()))
                .ReturnsAsync((TicketType t) =>
                {
                    t.TicketTypeId = 1;
                    return t;
                });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Premium");
            result.IsActive.Should().BeTrue();
            _mockTicketTypeRepository.Verify(x => x.CreateAsync(It.Is<TicketType>(t =>
                t.Name == createDto.Name && t.IsActive == createDto.IsActive)), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that CreateAsync throws exception when ticket type name already exists.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DuplicateName_ThrowsArgumentException()
        {
            // Arrange
            var createDto = new CreateTicketTypeDto { Name = "VIP", IsActive = true };
            var existingTicketType = new TicketType { TicketTypeId = 1, Name = "VIP" };

            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(createDto.Name))
                .ReturnsAsync(existingTicketType);

            // Act
            Func<Task> act = async () => await _sut.CreateAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Ticket type with this name already exists.");
        }

        /// <summary>
        /// Verifies that CreateAsync creates ticket type with various names.
        /// </summary>
        [Theory]
        [InlineData("VIP")]
        [InlineData("Regular")]
        [InlineData("Student")]
        [InlineData("Early Bird")]
        [InlineData("Group Pass")]
        public async Task CreateAsync_VariousNames_CreatesSuccessfully(string ticketTypeName)
        {
            // Arrange
            var createDto = new CreateTicketTypeDto { Name = ticketTypeName, IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(createDto.Name))
                .ReturnsAsync((TicketType?)null);
            _mockTicketTypeRepository.Setup(x => x.CreateAsync(It.IsAny<TicketType>()))
                .ReturnsAsync((TicketType t) =>
                {
                    t.TicketTypeId = 1;
                    return t;
                });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateAsync(createDto);

            // Assert
            result.Name.Should().Be(ticketTypeName);
        }

        #endregion

        #region UpdateAsync Tests

        /// <summary>
        /// Verifies that UpdateAsync successfully updates an existing ticket type.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ValidData_UpdatesTicketType()
        {
            // Arrange
            var updateDto = new UpdateTicketTypeDto
            {
                TicketTypeId = 1,
                Name = "Updated VIP",
                IsActive = false
            };
            var existingTicketType = new TicketType { TicketTypeId = 1, Name = "VIP", IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(updateDto.TicketTypeId))
                .ReturnsAsync(existingTicketType);
            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(updateDto.Name))
                .ReturnsAsync((TicketType?)null);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateAsync(updateDto);

            // Assert
            existingTicketType.Name.Should().Be("Updated VIP");
            existingTicketType.IsActive.Should().BeFalse();
            _mockTicketTypeRepository.Verify(x => x.Update(existingTicketType), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that UpdateAsync throws exception when ticket type is not found.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_TicketTypeNotFound_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new UpdateTicketTypeDto
            {
                TicketTypeId = 999,
                Name = "Non-existent",
                IsActive = true
            };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(updateDto.TicketTypeId))
                .ReturnsAsync((TicketType?)null);

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(updateDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Ticket type not found.");
        }

        /// <summary>
        /// Verifies that UpdateAsync throws exception when updating to duplicate name.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DuplicateName_ThrowsArgumentException()
        {
            // Arrange
            var updateDto = new UpdateTicketTypeDto
            {
                TicketTypeId = 1,
                Name = "Regular",
                IsActive = true
            };
            var existingTicketType = new TicketType { TicketTypeId = 1, Name = "VIP", IsActive = true };
            var otherTicketType = new TicketType { TicketTypeId = 2, Name = "Regular", IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(updateDto.TicketTypeId))
                .ReturnsAsync(existingTicketType);
            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(updateDto.Name))
                .ReturnsAsync(otherTicketType);

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(updateDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Another ticket type with this name already exists.");
        }

        /// <summary>
        /// Verifies that UpdateAsync allows keeping the same name when updating other properties.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_SameNameDifferentProperties_UpdatesSuccessfully()
        {
            // Arrange
            var updateDto = new UpdateTicketTypeDto
            {
                TicketTypeId = 1,
                Name = "VIP",
                IsActive = false // Just changing IsActive
            };
            var existingTicketType = new TicketType { TicketTypeId = 1, Name = "VIP", IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(updateDto.TicketTypeId))
                .ReturnsAsync(existingTicketType);
            _mockTicketTypeRepository.Setup(x => x.GetByNameAsync(updateDto.Name))
                .ReturnsAsync(existingTicketType); // Same ticket type returned by name search
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateAsync(updateDto);

            // Assert
            existingTicketType.IsActive.Should().BeFalse();
            _mockTicketTypeRepository.Verify(x => x.Update(existingTicketType), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        /// <summary>
        /// Verifies that DeleteAsync successfully deletes an existing ticket type.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ValidId_DeletesTicketType()
        {
            // Arrange
            var ticketTypeId = 1;
            var ticketType = new TicketType { TicketTypeId = ticketTypeId, Name = "To Delete" };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync(ticketType);
            _mockTicketTypeRepository.Setup(x => x.DeleteAsync(ticketTypeId))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteAsync(ticketTypeId);

            // Assert
            _mockTicketTypeRepository.Verify(x => x.DeleteAsync(ticketTypeId), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteAsync throws exception when ticket type is not found.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_TicketTypeNotFound_ThrowsArgumentException()
        {
            // Arrange
            var ticketTypeId = 999;

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync((TicketType?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(ticketTypeId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Ticket type not found.");
        }

        #endregion

        #region ToggleActiveStatusAsync Tests

        /// <summary>
        /// Verifies that ToggleActiveStatusAsync toggles status from active to inactive.
        /// </summary>
        [Fact]
        public async Task ToggleActiveStatusAsync_ActiveTicketType_BecomesInactive()
        {
            // Arrange
            var ticketTypeId = 1;
            var ticketType = new TicketType { TicketTypeId = ticketTypeId, Name = "VIP", IsActive = true };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync(ticketType);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.ToggleActiveStatusAsync(ticketTypeId);

            // Assert
            ticketType.IsActive.Should().BeFalse();
            _mockTicketTypeRepository.Verify(x => x.Update(ticketType), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that ToggleActiveStatusAsync toggles status from inactive to active.
        /// </summary>
        [Fact]
        public async Task ToggleActiveStatusAsync_InactiveTicketType_BecomesActive()
        {
            // Arrange
            var ticketTypeId = 1;
            var ticketType = new TicketType { TicketTypeId = ticketTypeId, Name = "VIP", IsActive = false };

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync(ticketType);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.ToggleActiveStatusAsync(ticketTypeId);

            // Assert
            ticketType.IsActive.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that ToggleActiveStatusAsync throws exception when ticket type is not found.
        /// </summary>
        [Fact]
        public async Task ToggleActiveStatusAsync_TicketTypeNotFound_ThrowsArgumentException()
        {
            // Arrange
            var ticketTypeId = 999;

            _mockTicketTypeRepository.Setup(x => x.GetByIdAsync(ticketTypeId))
                .ReturnsAsync((TicketType?)null);

            // Act
            Func<Task> act = async () => await _sut.ToggleActiveStatusAsync(ticketTypeId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Ticket type not found.");
        }

        #endregion
    }
}
