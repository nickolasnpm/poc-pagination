using Microsoft.EntityFrameworkCore;
using pagination.Application;
using pagination.Domain;
using Pagination.Infrastructure;

namespace pagination.Infrastructure
{
    public interface ICursorRepository: IBaseRepository<User, CursorPaginationRequest>
    {

    }

    public class CursorRepository : ICursorRepository
    {
        public readonly UserDbContext _userDbContext;
        public CursorRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public async Task<(ICollection<User>, int)> GetAsync(CursorPaginationRequest request)
        {
            var queryable = _userDbContext.Users.Where(u => u.IsActive == true).AsQueryable();
            var lastSeenId = request.Cursor;
            var totalCount = 0;

            if (request.IsQueryPreviousPage)
            {
                lastSeenId = request.Cursor - (request.PageSize * 2);
            }

            if (request.IsIncludeTotalCount)
            {
                totalCount = await queryable.CountAsync();
            }

            return ((await queryable.OrderBy(u => u.Id).Where(u => u.Id > lastSeenId).Take(request.PageSize + 1).ToListAsync()), totalCount);
            // by right, cursor pagination does not care about total count
            // it assumes user does not need to know total number or even what page they are currently in
            // for hybrid pagination model, may consider storing totalcount in cache 
        }

        public Task<(ICollection<User>, int)> GetWithPrefetchStrategyAsync(CursorPaginationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
