using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService 
        : IDataService<Asset, AssetEntity>
    {
        Task<AssetTree> GetAssetTreeAsync(
            string userId,
            string assetId,
            CancellationToken ct);

        Task<IEnumerable<PathUnit>> GetAssetPathAsync(
            string userId,
            string assetId,
            CancellationToken ct);

        Task<IEnumerable<Asset>> GetAssetsByCategoryId(
            string userId,
            string categoryId,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> CreateRootAssetForUserAsync(
            string userId,
            CancellationToken ct);
    }
}
