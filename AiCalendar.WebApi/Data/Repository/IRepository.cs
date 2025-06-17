using System.Linq.Expressions;

namespace AiCalendar.WebApi.Data.Repository
{
    public interface IRepository<TEntity>  where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id);
        Task<TEntity?> GetByExpressionAsync(Expression<Func<TEntity, bool>> expression);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllByExpressionAsync(Expression<Func<TEntity, bool>> expression);
        Task AddAsync(TEntity entity);
        void UpdateAsync(TEntity entity);
        Task DeleteAsync(Guid id);

        void DeleteEntity(TEntity entity);
        
        Task<bool> ExistsByIdAsync(Guid id);
        Task<bool> ExistsByExpressionAsync(Expression<Func<TEntity, bool>> expression);
        Task<int> CountAsync();
        Task SaveChangesAsync();

        IQueryable<TEntity> WithIncludes(params Expression<Func<TEntity, object>>[] includes);

        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
