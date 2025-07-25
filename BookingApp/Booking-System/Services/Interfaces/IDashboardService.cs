using Booking_System.DTOs;

namespace Booking_System.Business_Logic.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
    }
}
