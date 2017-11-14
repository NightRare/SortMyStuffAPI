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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : 
        EntityResourceBaseController<Asset, AssetEntity>
    {
        private readonly IAssetDataService _assetDataService;
        private readonly ICategoryDataService _categoryDataService;

        public AssetsController(
            IAssetDataService assetDataService,
            ICategoryDataService categoryDataService, 
            IUserDataService userDataService,
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IHostingEnvironment env,
            IAuthorizationService authService) : 
            base(assetDataService, 
                pagingOptions, 
                apiConfigs, 
                userDataService,
                env,
                authService)
        {
            _assetDataService = assetDataService;
            _categoryDataService = categoryDataService;
        }

        // GET /assets
        [Authorize]
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
        [Authorize]
        [HttpGet("{assetId}", Name = nameof(GetAssetByIdAsync))]
        public async Task<IActionResult> GetAssetByIdAsync(string assetId, CancellationToken ct)
        {
            return await GetResourceByIdAsync(assetId, ct);
        }

        // GET /assets/{assetId}/path
        [Authorize]
        [HttpGet("{assetId}/path", Name = nameof(GetAssetPathByIdAsync))]
        public async Task<IActionResult> GetAssetPathByIdAsync(
            string assetId,
            CancellationToken ct)
        {
            string userId = await GetUserId();

            IEnumerable<PathUnit> pathUnits;
            try
            {
                pathUnits = await _assetDataService.GetAssetPathAsync(userId, assetId, ct);
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
        [Authorize]
        [HttpPost(Name = nameof(CreateAssetAsync))]
        public async Task<IActionResult> CreateAssetAsync(
            [FromBody] CreateAssetForm body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            string userId = await GetUserId();

            ApiError error;
            if ((error = await CheckBaseForm(userId, body, ct)) != null)
                return BadRequest(error);

            var currentTime = DateTimeOffset.UtcNow;
            Action<Asset> operation = asset =>
            {
                asset.CreateTimestamp = currentTime;
                asset.ModifyTimestamp = currentTime;
            };

            return await CreateResourceAsync(
                nameof(GetAssetByIdAsync),
                body,
                ct, 
                operation);
        }

        // PUT /assets/{assetId}
        [Authorize]
        [HttpPut("{assetId}", Name = nameof(AddOrUpdateAssetAsync))]
        public async Task<IActionResult> AddOrUpdateAssetAsync(
            string assetId,
            [FromBody] AddOrUpdateAssetForm body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var user = await UserService.GetUserByIdAsync(await GetUserId());

            if (assetId.Equals(user?.RootAssetId))
                return BadRequest(new ApiError("Cannot create or modify the root asset."));

            ApiError error;
            if ((error = await CheckBaseForm(user.Id, body, ct)) != null)
                return BadRequest(error);

            if (DateTimeOffset.Compare(body.CreateTimestamp.Value, body.ModifyTimestamp.Value) > 0)
                return BadRequest(new ApiError("The create timestamp cannot be later than the modify timestamp."));

            var record = await _assetDataService.GetResourceAsync(user.Id, assetId, ct);

            // [Start Create Asset]
            if (record == null)
            {
                user.Id = await GetUserId();

                // newly added asset via api must have a correctly formatted guid
                if (!Guid.TryParse(assetId, out var guid))
                    return BadRequest(new ApiError("The assetId must be a correctly formatted Guid."));

                var asset = Mapper.Map<AddOrUpdateAssetForm, Asset>(body);
                asset.Id = assetId;

                var createResult = await _assetDataService.AddResourceAsync(user.Id, asset, ct);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new ApiError("Create asset failed.", createResult.Error));
                }

                return Ok();
            }
            // [End Create Asset]

            if (DateTimeOffset.Compare(record.CreateTimestamp, body.CreateTimestamp.Value) != 0)
                return BadRequest(new ApiError("CreateTimestamp cannot be modified."));

            // TODO: to allow change Category
            if (!record.CategoryId.Equals(body.CategoryId))
                return BadRequest(new ApiError("Category cannot be modified."));

            record.Name = body.Name;
            record.ModifyTimestamp = body.ModifyTimestamp.Value;
            record.ContainerId = body.ContainerId;

            var updateResult = await _assetDataService.UpdateAssetAsync(user.Id, record, ct);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new ApiError("Update asset failed.", updateResult.Error));
            }

            return Ok();
        }

        // DELETE /assets/{assetId}
        [Authorize]
        [HttpDelete("{assetId}", Name = nameof(DeleteAssetByIdAsync))]
        public async Task<IActionResult> DeleteAssetByIdAsync(
            string assetId,
            [FromQuery] DeletingAssetOptions deletingOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var user = await UserService.GetUserByIdAsync(await GetUserId());

            if (assetId.Equals(user?.RootAssetId))
                return BadRequest(new ApiError("Cannot delete the root asset"));

            try
            {
                await _assetDataService.DeleteAssetAsync(
                    user.Id,
                    assetId, 
                    deletingOptions.OnlySelf, 
                    ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        #region PRIVATE METHODS

        private async Task<ApiError> CheckBaseForm(string userId, BaseAssetForm form, CancellationToken ct)
        {
            var category = await _categoryDataService.GetResourceAsync(userId, form.CategoryId, ct);
            if (category == null)
                return new ApiError("Category not found, please create category first.");

            var container = await _assetDataService.GetResourceAsync(userId, form.ContainerId, ct);
            if (container == null)
                return new ApiError("Container not found.");

            return null;
        }

        #endregion
    }
}
