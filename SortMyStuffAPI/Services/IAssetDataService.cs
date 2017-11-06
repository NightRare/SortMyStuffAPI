using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models.Resources;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService : IDataService
    {
        Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct);

        Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken ct);

        Task<Asset> GetAssetAsync(string id, CancellationToken ct);
    }
}
