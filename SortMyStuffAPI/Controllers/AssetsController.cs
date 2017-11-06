using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models.Resources;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : Controller
    {

        private readonly IAssetDataService _ads;

        public AssetsController(IAssetDataService assetDataService)
        {
            _ads = assetDataService;
        }

        [HttpGet(Name = nameof(GetAssetsAsync))]
        public async Task<IActionResult> GetAssetsAsync(CancellationToken ct)
        {
            var assets = await _ads.GetAssetsAsync(ct);
            
            var response = new Collection<Asset>
            {
                Self = Link.ToCollection(nameof(GetAssetsAsync)),
                Value = assets.ToArray()
            };

            return Ok(response);
        }

        [HttpGet("{assetId}", Name = nameof(GetAssetByIdAsync))]
        public async Task<IActionResult> GetAssetByIdAsync(string assetId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
