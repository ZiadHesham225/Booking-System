using Booking_System.Data;
using Booking_System.DTOs;
using Booking_System.Models;

namespace Booking_System.Business_Logic.Interfaces
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TicketTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TicketTypeDto>> GetAllAsync()
        {
            var ticketTypes = await _unitOfWork.TicketTypes.GetAllAsync();
            return ticketTypes.Select(MapToDto);
        }

        public async Task<IEnumerable<TicketTypeDto>> GetActiveTicketTypesAsync()
        {
            var ticketTypes = await _unitOfWork.TicketTypes.GetActiveTicketTypesAsync();
            return ticketTypes.Select(MapToDto);
        }

        public async Task<TicketTypeDto> GetByIdAsync(int id)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
            if (ticketType == null) return null;

            return MapToDto(ticketType);
        }

        public async Task<TicketTypeDto> GetByNameAsync(string name)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByNameAsync(name);
            if (ticketType == null) return null;

            return MapToDto(ticketType);
        }

        public async Task<TicketTypeDto> CreateAsync(CreateTicketTypeDto dto)
        {
            var existingTicketType = await _unitOfWork.TicketTypes.GetByNameAsync(dto.Name);
            if (existingTicketType != null)
                throw new ArgumentException("Ticket type with this name already exists.");

            var ticketType = new TicketType
            {
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            await _unitOfWork.TicketTypes.CreateAsync(ticketType);
            await _unitOfWork.CommitAsync();

            return MapToDto(ticketType);
        }

        public async Task UpdateAsync(UpdateTicketTypeDto dto)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(dto.TicketTypeId);
            if (ticketType == null)
                throw new ArgumentException("Ticket type not found.");

            var existingTicketType = await _unitOfWork.TicketTypes.GetByNameAsync(dto.Name);
            if (existingTicketType != null && existingTicketType.TicketTypeId != dto.TicketTypeId)
                throw new ArgumentException("Another ticket type with this name already exists.");

            ticketType.Name = dto.Name;
            ticketType.IsActive = dto.IsActive;

            _unitOfWork.TicketTypes.Update(ticketType);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
            if (ticketType == null)
                throw new ArgumentException("Ticket type not found.");

            await _unitOfWork.TicketTypes.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }

        public async Task ToggleActiveStatusAsync(int id)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
            if (ticketType == null)
                throw new ArgumentException("Ticket type not found.");

            ticketType.IsActive = !ticketType.IsActive;
            _unitOfWork.TicketTypes.Update(ticketType);
            await _unitOfWork.CommitAsync();
        }

        private TicketTypeDto MapToDto(TicketType ticketType)
        {
            return new TicketTypeDto
            {
                TicketTypeId = ticketType.TicketTypeId,
                Name = ticketType.Name,
                IsActive = ticketType.IsActive
            };
        }
    }
}
