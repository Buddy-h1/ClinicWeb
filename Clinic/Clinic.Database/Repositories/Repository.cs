using Microsoft.EntityFrameworkCore;

namespace Clinic.Database.Repositories
{
    public abstract class Repository<D, T> : IDisposable 
        where D : DbContext
        where T : class
    {
        public Repository(D context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        protected readonly D _context;
        protected readonly DbSet<T> _set;

        virtual public async Task<bool> SaveAsync(T entity)
        {
            try
            {
                var entry = _context.Entry(entity);

                if (entry.State == EntityState.Detached)
                {
                    _set.Add(entity);
                }
                else if (entry.State == EntityState.Modified)
                {
                    _set.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                }

                if (await _context.SaveChangesAsync() > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        virtual public async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                _set.Remove(entity);
                if (await _context.SaveChangesAsync() > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IQueryable<T> GetAll()
        {
            return _set;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
