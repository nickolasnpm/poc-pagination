using Microsoft.EntityFrameworkCore;
using pagination.Application;
using pagination.Domain;
using Pagination.Infrastructure;
using System.Linq;

namespace pagination.Infrastructure
{
    public interface IOffsetRepository: IBaseRepository<User, OffsetPaginationRequest>
    {
       
    }

    public class OffsetRepository : IOffsetRepository
    {
        private readonly UserDbContext _userDbContext;
        public OffsetRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public async Task<(ICollection<User>, int)> GetAsync(OffsetPaginationRequest request)
        {
            var queryable = _userDbContext.Users.AsNoTracking().Where(u => u.IsActive == true).AsQueryable();

            return (await queryable.OrderBy(u => u.Id).Skip((request.Page! - 1) * request.PageSize).Take(request.PageSize).ToListAsync(), await queryable.CountAsync());
            // queryable.Count() seems unnecessary to be done in every call. 
            // can consider storing it in cache that runs on intervals
            // pros: only 1 db call, faster retrieval
            // cons: need to handle cached object lifecycle (more work to do), stale data as list only gets refreshed between intervals 
        }

        public Task<(ICollection<User>, int)> GetWithPrefetchStrategyAsync(OffsetPaginationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
