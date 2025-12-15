using Booking_System.Application.DTOs.Admin;

namespace Booking_System.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
    }
}



