namespace Booking_System.Data_Access.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task<T> CreateAsync(T entity);
        void Update(T entity);
        Task DeleteAsync(object id);
        Task SaveAsync();
    }
}
