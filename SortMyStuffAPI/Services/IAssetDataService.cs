using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Models.QueryOptions;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService : IDataService
    {
        Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct);

        Task<PagedResults<Asset>> GetAllAssetsAsync(
            CancellationToken ct, 
            PagingOptions pagingOptions = null,
            SortOptions<Asset, AssetEntity> sortOptions = null,
            SearchOptions<Asset, AssetEntity> searchOptions = null);

        Task<Asset> GetAssetAsync(string id, CancellationToken ct);

        Task<int> UpdateAssetAsync(
            string id, 
            CancellationToken ct, 
            string name = null, 
            string containerId = null, 
            string category = null,
            string modifyTimestamp = null);
    }
}
