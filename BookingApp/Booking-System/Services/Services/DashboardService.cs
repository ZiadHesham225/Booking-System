using Booking_System.Data;
using Booking_System.DTOs;
using Booking_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Business_Logic.Interfaces
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            try
            {
                var totalUsers = await _userManager.Users.CountAsync();
                var totalBookings = await _unitOfWork.Bookings.CountAsync();
                var todayBookings = await _unitOfWork.Bookings.CountAsync(b => b.BookingDate.Date == DateTime.UtcNow.Date);
                var totalRevenue = await _unitOfWork.Bookings.SumAsync(b => (decimal?)b.TotalPrice);
                var totalEvents = await _unitOfWork.Events.CountAsync();
                var topEvents = await _unitOfWork.Events.GetTopBookedEventsAsync(5);

                return new AdminDashboardDto
                {
                    TotalUsers = totalUsers,
                    TotalBookings = totalBookings,
                    TodayBookings = todayBookings,
                    TotalRevenue = totalRevenue,
                    TotalEvents = totalEvents,
                    TopEvents = topEvents
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve dashboard data", ex);
            }
        }
    }
}
