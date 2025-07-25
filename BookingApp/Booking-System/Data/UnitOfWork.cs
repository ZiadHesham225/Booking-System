using Booking_System.Data_Access.Interfaces;
using Booking_System.Data_Access.Repositories;

namespace Booking_System.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IEventRepository? _events;
        private ICategoryRepository? _categories;
        private IRefreshTokenRepository? _refreshTokens;
        private IBookingRepository? _Bookings;
        private IUserCouponRepository? _userCoupons;
        private ICouponRepository? _coupons;
        private ITicketTypeRepository? _ticketTypes;
        private IEventTicketTypeRepository? _eventTicketTypes;

        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public IEventRepository Events => _events ??= new EventRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
        public IBookingRepository Bookings => _Bookings ??= new BookingRepository(_context);
        public IUserCouponRepository UserCoupons => _userCoupons ??= new UserCouponRepository(_context);
        public ICouponRepository Coupons => _coupons ??= new CouponRepository(_context);
        public ITicketTypeRepository TicketTypes => _ticketTypes ??= new TicketTypeRepository(_context);
        public IEventTicketTypeRepository EventTicketTypes => _eventTicketTypes ??= new EventTicketTypeRepository(_context);

        public async Task CommitAsync()
        {
           await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
