using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface ICategoryDataService : IDataService<Category, CategoryEntity>
    {
        Task<Category> GetCategoryByNameAsync(
            string userId,
            string name, 
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> AddCategoryAsync(
            string userId,
            Category catgeory,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> UpdateCategoryAsync(
            string userId,
            Category catgeory,
            CancellationToken ct);

        Task DeleteCategoryAsync(
            string userId,
            string id, 
            CancellationToken ct);
    }
}
