using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService : IDataService<Asset, AssetEntity>
    {
        Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct);

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
