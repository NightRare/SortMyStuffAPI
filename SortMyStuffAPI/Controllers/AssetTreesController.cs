using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetTreesController : Controller
    {
        private readonly IAssetDataService _ads;

        public AssetTreesController(IAssetDataService assetDataService)
        {
            _ads = assetDataService;
        }

        [HttpGet(Name = nameof(GetAssetTree))]
        public async Task<IActionResult> GetAssetTree()
        {
            var rootTree = await _ads.GetAssetTreeAsync(ApiStrings.ROOT_ASSET_ID, CancellationToken.None);

            return Ok(rootTree);
        }

        [HttpGet("{assetTreeId}", Name = nameof(GetAssetTreeByIdAsync))]
        public async Task<IActionResult> GetAssetTreeByIdAsync(string assetTreeId, CancellationToken ct)
        {
            var assetTree = await _ads.GetAssetTreeAsync(assetTreeId, ct);

            return Ok(assetTree);
        }

    }

}
