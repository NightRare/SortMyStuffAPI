using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetTreesController : Controller
    {
        private readonly IAssetDataService _ads;
        private readonly ApiConfigs _apiConfigs;

        public AssetTreesController(IAssetDataService assetDataService, IOptions<ApiConfigs> apiConfigs)
        {
            _ads = assetDataService;
            _apiConfigs = apiConfigs.Value;
        }

        [HttpGet(Name = nameof(GetAssetTreeAsync))]
        public async Task<IActionResult> GetAssetTreeAsync(CancellationToken ct)
        {
            var rootTree = await _ads.GetAssetTreeAsync(_apiConfigs.RootAssetId, ct);

            var response = new Collection<AssetTree>
            {
                Self = Link.ToCollection(nameof(GetAssetTreeAsync)),
                Value = new AssetTree[] { rootTree }
            };

            return Ok(response);
        }

        [HttpGet("{assetTreeId}", Name = nameof(GetAssetTreeByIdAsync))]
        public async Task<IActionResult> GetAssetTreeByIdAsync(string assetTreeId, CancellationToken ct)
        {
            var assetTree = await _ads.GetAssetTreeAsync(assetTreeId, ct);

            return Ok(assetTree);
        }

    }

}
