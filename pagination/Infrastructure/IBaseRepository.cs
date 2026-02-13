using pagination.Application;
using pagination.Domain;

namespace Pagination.Infrastructure
{
    public interface IBaseRepository<TEntity, TRequest> where TEntity : class where TRequest : class
    {
        Task<(ICollection<TEntity> Items, int TotalCount)> GetAsync(TRequest request);
        Task<(ICollection<TEntity> Items, int TotalCount)> GetWithPrefetchStrategyAsync(TRequest request);
    }
}

