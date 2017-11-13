using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;

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

        // GET /assettrees/
        [Authorize]
        [HttpGet(Name = nameof(GetAssetTreesAsync))]
        public async Task<IActionResult> GetAssetTreesAsync(CancellationToken ct)
        {
            var rootTree = await _ads.GetAssetTreeAsync(_apiConfigs.RootAssetId, ct);

            var response = new Collection<AssetTree>
            {
                Self = Link.ToCollection(nameof(GetAssetTreesAsync)),
                Value = new AssetTree[] { rootTree }
            };

            return Ok(response);
        }

        // GET /assettrees/{assetId}
        [Authorize]
        [HttpGet("{assetTreeId}", Name = nameof(GetAssetTreeByIdAsync))]
        public async Task<IActionResult> GetAssetTreeByIdAsync(string assetTreeId, CancellationToken ct)
        {
            var assetTree = await _ads.GetAssetTreeAsync(assetTreeId, ct);
            if (assetTree == null) return NotFound();

            return Ok(assetTree);
        }

    }

}
