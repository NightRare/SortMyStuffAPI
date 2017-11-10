using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface ICategoryDataService : IDataService
    {
        Task<PagedResults<Category>> GetAllCategoriesAsync(
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Category, CategoryEntity> sortOptions = null,
            SearchOptions<Category, CategoryEntity> searchOptions = null);

        Task<Category> GetCategoryAsync(string id, CancellationToken ct);

        Task AddOrUpdateAssetAsync(Category category, CancellationToken ct);

        Task DeleteCategoryAsync(string id, CancellationToken ct);
    }
}
