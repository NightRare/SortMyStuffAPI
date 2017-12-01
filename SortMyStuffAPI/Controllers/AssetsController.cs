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
using SortMyStuffAPI.Utils;
using SortMyStuffAPI.Exceptions;
using System.Net;
using SortMyStuffAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController :
        EntityResourceBaseController<Asset, AssetEntity>
    {
        private readonly IAssetDataService _assetDataService;
        private readonly ICategoryDataService _categoryDataService;
        private readonly IBaseDetailDataService _baseDetailDataService;
        private readonly IDetailDataService _detailDataService;
        private readonly IDataChangeSender _dataChangeSender;

        public AssetsController(
            IAssetDataService assetDataService,
            ICategoryDataService categoryDataService,
            IUserService userService,
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IHostingEnvironment env,
            IAuthorizationService authService,
            IBaseDetailDataService baseDetailDataService,
            IDetailDataService detailDataService,
            IDataChangeSender dataChangeSender) :
            base(assetDataService,
                pagingOptions,
                apiConfigs,
                userService,
                env,
                authService)
        {
            _assetDataService = assetDataService;
            _categoryDataService = categoryDataService;
            _baseDetailDataService = baseDetailDataService;
            _detailDataService = detailDataService;
            _dataChangeSender = dataChangeSender;
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
            var errorMsg = "GET assets failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            var result = await GetResourcesAsync(
                nameof(GetAssetsAsync),
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);

            return GetActionResult(result, errorMsg);
        }

        // GET /assets/{assetId}
        [Authorize]
        [HttpGet("{assetId}", Name = nameof(GetAssetByIdAsync))]
        public async Task<IActionResult> GetAssetByIdAsync(
            string assetId, CancellationToken ct)
        {
            return GetActionResult(
                await GetResourceByIdAsync(assetId, ct));
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
                pathUnits = await _assetDataService.GetAssetPathAsync(
                    userId, assetId, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            var response = new Collection<PathUnit>
            {
                Self = Link.ToCollection(nameof(GetAssetPathByIdAsync),
                    new { assetId = assetId }),
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

            var errorMsg = "POST asset failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            string userId = await GetUserId();

            var checkResult = await CheckBaseForm(userId, body, ct);
            if (!checkResult.IsValid)
            {
                return BadRequest(new ApiError(
                    errorMsg, checkResult.Error));
            }

            var currentTime = DateTimeOffset.UtcNow;
            Asset asset = null;
            Action<Asset> operation = a =>
            {
                a.CreateTimestamp = currentTime;
                a.ModifyTimestamp = currentTime;
                asset = a;
            };

            var result = await CreateResourceAsync(
                Guid.NewGuid().ToString(), body, ct, operation);

            if (result.Succeeded)
            {
                var addDetails = await GenerateDetailsToAssetAsync(userId, asset, ct);
                if (!addDetails.Succeeded)
                {
                    return BadRequest(new ApiError(errorMsg, addDetails.Error));
                }
            }

            return GetActionResult(result, errorMsg);
        }

        // PUT /assets/{assetId}
        [Authorize]
        [HttpPut("{assetId}", Name = nameof(AddOrUpdateAssetAsync))]
        public async Task<IActionResult> AddOrUpdateAssetAsync(
            string assetId,
            [FromBody] FormMode body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var errorMsg = "PUT asset failed.";

            var userId = await GetUserId();
            var user = await UserService.GetUserByIdAsync(userId) ??
                throw new ApiException($"User not exists:[{userId}]");

            if (assetId.Equals(user.RootAssetId))
            {
                return BadRequest(new ApiError(
                    errorMsg,
                    "Cannot create or modify the root asset."));
            }

            var checkResult = await CheckBaseForm(user.Id, body, ct);
            if (!checkResult.IsValid)
            {
                return BadRequest(new ApiError(
                    errorMsg,
                    checkResult.Error));
            }

            if (DateTimeOffset.Compare(
                body.CreateTimestamp.Value, body.ModifyTimestamp.Value) > 0)
            {
                return BadRequest(new ApiError(
                    errorMsg,
                    "The modify timestamp cannot be earlier than the create timestamp."));
            }

            var record = await DataService.GetResourceAsync(userId, assetId, ct);

            if (record == null)
            {
                Asset asset = null;
                Action<Asset> operation = a => asset = a;
                var result = await CreateResourceAsync(
                    assetId, body, ct, operation);

                if (result.Succeeded)
                {
                    var addDetails = await GenerateDetailsToAssetAsync(userId, asset, ct);
                    if (!addDetails.Succeeded)
                    {
                        return BadRequest(new ApiError(errorMsg, addDetails.Error));
                    }
                }

                return GetActionResult(result, errorMsg);
            }

            return GetActionResult(await UpdateResourceAsync(record, body, ct));
        }

        // DELETE /assets/{assetId}
        [Authorize]
        [HttpDelete("{assetId}", Name = nameof(DeleteAssetByIdAsync))]
        public async Task<IActionResult> DeleteAssetByIdAsync(
            string assetId,
            [FromQuery] DeletingOptions deletingOptions,
            CancellationToken ct)
        {
            var errorMsg = "Delete asset failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            var userId = await GetUserId();
            var user = await UserService.GetUserByIdAsync(userId) ??
                throw new ApiException($"User not exists:[{userId}]");

            if (assetId.Equals(user?.RootAssetId))
            {
                return BadRequest(new ApiError(
                    errorMsg,
                    "Cannot delete the root asset"));
            }

            try
            {
                var result = await _assetDataService.DeleteResourceAsync(
                    user.Id,
                    assetId,
                    deletingOptions.DelDependents);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiError(
                        errorMsg,
                        result.Error));
                }
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        #region PRIVATE METHODS

        private async Task<(bool IsValid, string Error)> CheckBaseForm(
            string userId,
            AssetBaseForm form,
            CancellationToken ct)
        {
            var category = await _categoryDataService.GetResourceAsync(userId, form.CategoryId, ct);
            if (category == null)
                return (false, "Category not found, please create category first.");

            var container = await _assetDataService.GetResourceAsync(userId, form.ContainerId, ct);
            if (container == null)
                return (false, "Container not found.");

            return (true, null);
        }


        private async Task<(bool Succeeded, string Error)> GenerateDetailsToAssetAsync(
            string userId,
            Asset asset,
            CancellationToken ct)
        {
            var currentTime = DateTimeOffset.UtcNow;

            var baseDetails = await _baseDetailDataService
                .GetBaseDetailsByCategoryIdAsync(
                    userId, asset.CategoryId, ct);

            var details = baseDetails.Select(baseDetail =>
                new Detail
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BaseId = baseDetail.Id,
                    AssetId = asset.Id,
                    ModifyTimestamp = currentTime,
                    Field = null
                });

            var addDetail = await _detailDataService.AddResourceCollectionAsync(
                userId, details.ToArray(), ct);

            if (!addDetail.Succeeded)
            {
                return (false,
                    $"Add details to assets failed.");
            }
            return (true, null);
        }

        #endregion
    }
}
