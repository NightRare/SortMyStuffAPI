using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models.QueryOptions;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : Controller
    {

        private readonly IAssetDataService _assetDataService;
        private readonly PagingOptions _defaultpagingOptions;

        public AssetsController(IAssetDataService assetDataService, IOptions<PagingOptions> pagingOptions)
        {
            _assetDataService = assetDataService;
            _defaultpagingOptions = pagingOptions.Value;
        }

        // GET /assets
        [HttpGet(Name = nameof(GetAssetsAsync))]
        public async Task<IActionResult> GetAssetsAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Asset, AssetEntity> sortOptions,
            [FromQuery] SearchOptions<Asset, AssetEntity> searchOptions)
        {
            // if any Model (in this case PagingOptions) property is not valid according to the Range attributes
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultpagingOptions.Offset;
            pagingOptions.PageSize = pagingOptions.PageSize ?? _defaultpagingOptions.PageSize;

            var assets = await _assetDataService.GetAllAssetsAsync(ct, pagingOptions, sortOptions, searchOptions);

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
            var asset = await _assetDataService.GetAssetAsync(assetId, ct);
            if (asset == null) return NotFound();

            return Ok(asset);
        }

        // GET /assets/{assetId}/path
        [HttpGet("{assetId}/path", Name = nameof(GetAssetPathByIDAsync))]
        public async Task<IActionResult> GetAssetPathByIDAsync(
            string assetId,
            CancellationToken ct)
        {
            IEnumerable<PathUnit> pathUnits;
            try
            {
                pathUnits = await _assetDataService.GetAssetPathAsync(assetId, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            var response = new Collection<PathUnit>
            {
                Self = Link.ToCollection(nameof(GetAssetPathByIDAsync), new {assetId = assetId}),
                Value = pathUnits.ToArray()
            };

            return Ok(response);
        }

        public async Task<IActionResult> CreateAsset(
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        // PUT /assets/{assetId}
        [HttpPut("{assetId}", Name = nameof(UpdateAssetByIdAsync))]
        public async Task<IActionResult> UpdateAssetByIdAsync(
            string assetId,
            [FromBody] AddOrUpdateAssetForm assetUpdatingForm,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var asset = await _assetDataService.GetAssetAsync(assetId, ct);
            if (asset == null) return NotFound();


            bool ok = await _assetDataService.UpdateAssetAsync(assetId, DateTimeOffset.UtcNow, ct, name: assetUpdatingForm.Name);
            return ok ? Ok() as IActionResult : BadRequest(new ApiError("Update database failed."));
        }
    }
}
