using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService : IDataService<Asset, AssetEntity>
    {
        Task<AssetTree> GetAssetTreeAsync(
            string userId,
            string id,
            CancellationToken ct);

        Task<IEnumerable<PathUnit>> GetAssetPathAsync(
            string userId,
            string id,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> AddAssetAsync(
            string userId,
            Asset asset,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> UpdateAssetAsync(
            string userId,
            Asset asset,
            CancellationToken ct);

        Task DeleteAssetAsync(
            string userId,
            string id,
            bool delOnlySelf,
            CancellationToken ct);
    }
}
