using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models.QueryOptions;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : Controller
    {

        private readonly IAssetDataService _assetDataService;
        private readonly PagingOptions _defaultpagingOptions;
        private readonly ApiConfigs _apiConfigs;

        public AssetsController(
            IAssetDataService assetDataService, 
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs)
        {
            _assetDataService = assetDataService;
            _defaultpagingOptions = pagingOptions.Value;
            _apiConfigs = apiConfigs.Value;
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
        [HttpGet("{assetId}/path", Name = nameof(GetAssetPathByIdAsync))]
        public async Task<IActionResult> GetAssetPathByIdAsync(
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
                Self = Link.ToCollection(nameof(GetAssetPathByIdAsync), new {assetId = assetId}),
                Value = pathUnits.ToArray()
            };

            return Ok(response);
        }

        // POST /assets
        [HttpPost(Name = nameof(CreateAssetAsync))]
        public async Task<IActionResult> CreateAssetAsync(
            [FromBody] CreateAssetForm body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            // TODO: check whether category exists

            var container = await _assetDataService.GetAssetAsync(body.ContainerId, ct);
            if (container == null) return BadRequest(new ApiError("Container not found."));

            var currentTime = DateTimeOffset.UtcNow;

            var asset = Mapper.Map<CreateAssetForm, Asset>(body);
            asset.Id = Guid.NewGuid().ToString();
            asset.CreateTimestamp = currentTime;
            asset.ModifyTimestamp = currentTime;

            await _assetDataService.AddOrUpdateAssetAsync(asset, ct);

            var assetCreated = await _assetDataService.GetAssetAsync(asset.Id, ct);

            return Created(
                Url.Link(nameof(GetAssetByIdAsync), 
                new { assetId = asset.Id }), 
                assetCreated);
        }

        // PUT /assets/{assetId}
        [HttpPut("{assetId}", Name = nameof(AddOrUpdateAssetAsync))]
        public async Task<IActionResult> AddOrUpdateAssetAsync(
            string assetId,
            [FromBody] AddOrUpdateAssetForm body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            if (assetId.Equals(_apiConfigs.RootAssetId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiError("Cannot create or modify the root asset."));

            // TODO: check whether category exists

            var container = await _assetDataService.GetAssetAsync(body.ContainerId, ct);
            if (container == null) return BadRequest(new ApiError("Container not found."));

            if (DateTimeOffset.Compare(body.CreateTimestamp.Value, body.ModifyTimestamp.Value) > 0)
                return BadRequest(new ApiError("The create timestamp cannot be later than the modify timestamp."));

            var record = await _assetDataService.GetAssetAsync(assetId, ct);
            if (record == null)
            {
                // newly added asset via api must have a correctly formatted guid
                if (!Guid.TryParse(assetId, out var result))
                    return BadRequest(new ApiError("The assetId must be a correctly formatted Guid."));

                var asset = Mapper.Map<AddOrUpdateAssetForm, Asset>(body);
                asset.Id = assetId;
                await _assetDataService.AddOrUpdateAssetAsync(asset, ct);
                return Ok();
            }

            if (DateTimeOffset.Compare(record.CreateTimestamp, body.CreateTimestamp.Value) != 0)
                return BadRequest(new ApiError("CreateTimestamp cannot be modified."));

            if (!record.Category.Equals(body.Category))
                return BadRequest(new ApiError("Category cannot be modified."));

            record.Name = body.Name;
            record.ModifyTimestamp = body.ModifyTimestamp.Value;
            record.ContainerId = body.ContainerId;

            await _assetDataService.AddOrUpdateAssetAsync(record, ct);

            return Ok();
        }

        // DELETE /assets/{assetId}
        [HttpDelete("{assetId}", Name = nameof(DeleteAssetByIdAsync))]
        public async Task<IActionResult> DeleteAssetByIdAsync(
            string assetId,
            [FromQuery] DeletingAssetOptions deletingOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            if (assetId.Equals(_apiConfigs.RootAssetId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiError("Cannot delete the root asset"));

            try
            {
                await _assetDataService.DeleteAssetAsync(assetId, deletingOptions.OnlySelf, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        #region PRIVATE METHODS

        #endregion
    }
}
