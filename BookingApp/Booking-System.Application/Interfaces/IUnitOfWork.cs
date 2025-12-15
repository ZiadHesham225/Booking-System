using Booking_System.Application.Interfaces;

namespace Booking_System.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IEventRepository Events { get; }
        ICategoryRepository Categories { get; }
        IBookingRepository Bookings { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        ITicketTypeRepository TicketTypes { get; }
        IEventTicketTypeRepository EventTicketTypes { get; }
        ICouponRepository Coupons { get; }
        IUserCouponRepository UserCoupons { get; }
        Task CommitAsync();
    }
}



