using Booking_System.Infrastructure.Data;
using Booking_System.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected DbSet<T> dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public async Task<T> CreateAsync(T entity)
        {
            var newEntity = await dbSet.AddAsync(entity);
            return newEntity.Entity;
        }

        public async Task CreateRangeAsync(IEnumerable<T> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public async Task DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                dbSet.Remove(entity);
            }
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
    }
}




