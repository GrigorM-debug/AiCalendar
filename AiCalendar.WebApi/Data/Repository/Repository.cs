using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<TEntity?> GetByExpressionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await _dbSet.Where(expression).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllByExpressionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void UpdateAsync(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            return await _dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public async Task<bool> ExistsByExpressionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<TEntity> WithIncludes(params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }
    }
}
