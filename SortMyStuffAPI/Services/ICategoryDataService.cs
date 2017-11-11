using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface ICategoryDataService : IDataService<Category, CategoryEntity>
    {
        Task<Category> GetCategoryByNameAsync(string name, CancellationToken ct);

        Task AddOrUpdateAssetAsync(Category category, CancellationToken ct);

        Task DeleteCategoryAsync(string id, CancellationToken ct);
    }
}
