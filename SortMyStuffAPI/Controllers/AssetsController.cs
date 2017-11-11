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

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : 
        JsonResourcesController<Asset, AssetEntity>
    {
        private readonly IAssetDataService _assetDataService;
        private readonly ICategoryDataService _categoryDataService;

        public AssetsController(
            IAssetDataService assetDataService,
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs, ICategoryDataService categoryDataService) : 
            base(assetDataService, pagingOptions, apiConfigs)
        {
            _assetDataService = assetDataService;
            _categoryDataService = categoryDataService;
        }
        
        // GET /assets
        [HttpGet(Name = nameof(GetAssetsAsync))]
        public async Task<IActionResult> GetAssetsAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Asset, AssetEntity> sortOptions,
            [FromQuery] SearchOptions<Asset, AssetEntity> searchOptions)
        {
            return await GetResourcesAsync(
                nameof(GetAssetsAsync),
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        // GET /assets/{assetId}
        [HttpGet("{assetId}", Name = nameof(GetAssetByIdAsync))]
        public async Task<IActionResult> GetAssetByIdAsync(string assetId, CancellationToken ct)
        {
            return await GetResourceByIdAsync(assetId, ct);
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

            ApiError error;
            if((error = await CheckBaseForm(body, ct)) != null)
                return BadRequest(error);

            var currentTime = DateTimeOffset.UtcNow;

            var asset = Mapper.Map<CreateAssetForm, Asset>(body);
            asset.Id = Guid.NewGuid().ToString();
            asset.CreateTimestamp = currentTime;
            asset.ModifyTimestamp = currentTime;

            await _assetDataService.AddOrUpdateAssetAsync(asset, ct);

            var assetCreated = await _assetDataService.GetResourceAsync(asset.Id, ct);

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

            if (assetId.Equals(ApiConfigs.RootAssetId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiError("Cannot create or modify the root asset."));

            ApiError error;
            if ((error = await CheckBaseForm(body, ct)) != null)
                return BadRequest(error);

            if (DateTimeOffset.Compare(body.CreateTimestamp.Value, body.ModifyTimestamp.Value) > 0)
                return BadRequest(new ApiError("The create timestamp cannot be later than the modify timestamp."));

            var record = await _assetDataService.GetResourceAsync(assetId, ct);
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

            if (!record.CategoryId.Equals(body.CategoryId))
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

            if (assetId.Equals(ApiConfigs.RootAssetId, StringComparison.OrdinalIgnoreCase))
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

        private async Task<ApiError> CheckBaseForm(BaseAssetForm form, CancellationToken ct)
        {
            var category = await _categoryDataService.GetResourceAsync(form.CategoryId, ct);
            if (category == null)
                return new ApiError("Category not found, please create category first.");

            var container = await _assetDataService.GetResourceAsync(form.ContainerId, ct);
            if (container == null)
                return new ApiError("Container not found.");

            return null;
        }

        #endregion
    }
}
