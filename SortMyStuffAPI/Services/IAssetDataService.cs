using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

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

        Task<IEnumerable<PathUnit>> GetAssetPathAsync(string id, CancellationToken ct);

        Task AddOrUpdateAssetAsync(
            Asset asset,
            CancellationToken ct);

        Task DeleteAssetAsync(
            string id,
            bool delOnlySelf,
            CancellationToken ct);
    }
}
