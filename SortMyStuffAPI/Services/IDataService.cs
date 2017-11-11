using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IDataService<T, TEntity> 
        where T : Resource 
        where TEntity : IEntity
    {
        Task<T> GetResourceAsync(string id, CancellationToken ct);

        Task<PagedResults<T>> GetResouceCollectionAsync(
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null);
    }
}
