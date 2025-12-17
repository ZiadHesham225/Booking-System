namespace Booking_System.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task<T> CreateAsync(T entity);
        Task CreateRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        Task DeleteAsync(object id);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task SaveAsync();
    }
}


