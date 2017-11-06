using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models.Resources;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface IAssetDataService : IDataService
    {
        Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct);

        Task<PagedResults<Asset>> GetAllAssetsAsync(CancellationToken ct, PagingOptions options = null);

        Task<Asset> GetAssetAsync(string id, CancellationToken ct);
    }
}
