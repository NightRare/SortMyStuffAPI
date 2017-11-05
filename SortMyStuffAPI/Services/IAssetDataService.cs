using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService : IDataService
    {
        Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct);

        Task<IList<Asset>> GetAssets(CancellationToken ct);

        Task<Asset> GetAsset(string id, CancellationToken ct);
    }
}
