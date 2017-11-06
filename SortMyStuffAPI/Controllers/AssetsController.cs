using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using Microsoft.Extensions.Options;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : Controller
    {

        private readonly IAssetDataService _ads;
        private readonly PagingOptions _defaultpagingOptions;

        public AssetsController(IAssetDataService assetDataService, IOptions<PagingOptions> pagingOptions)
        {
            _ads = assetDataService;
            _defaultpagingOptions = pagingOptions.Value;
        }

        // GET /assets
        [HttpGet(Name = nameof(GetAssetsAsync))]
        public async Task<IActionResult> GetAssetsAsync(
            CancellationToken ct, 
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Asset, AssetEntity> sortOptions)
        {
            // if any Model (in this case PagingOptions) property is not valid according to the Range attributes
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultpagingOptions.Offset;
            pagingOptions.PageSize = pagingOptions.PageSize ?? _defaultpagingOptions.PageSize;

            var assets = await _ads.GetAllAssetsAsync(ct, pagingOptions, sortOptions);

            var response = PagedCollection<Asset>.Create(
                Link.ToCollection(nameof(GetAssetsAsync)),
                assets.PagedItems.ToArray(),
                assets.TotalSize,
                pagingOptions);

            return Ok(response);
        }

        // GET /assets/{assetId}
        [HttpGet("{assetId}", Name = nameof(GetAssetByIdAsync))]
        public async Task<IActionResult> GetAssetByIdAsync(string assetId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
