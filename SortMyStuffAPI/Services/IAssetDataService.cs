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
            string id,
            CancellationToken ct);

        Task<IEnumerable<PathUnit>> GetAssetPathAsync(
            string userId,
            string id,
            CancellationToken ct);
    }
}
