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
                Self = Link.ToCollection(nameof(GetAssetPathByIdAsync), new { assetId = assetId }),
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

            var checkResult = await CheckBaseForm(userId, body, ct);
            if (!checkResult.IsValid)
            {
                return BadRequest(new ApiError(
                    "POST operation failed",
                    checkResult.Error));
            }

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
            [FromBody] FormMode body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var errorMsg = "PUT operation failed.";

            var user = await UserService.GetUserByIdAsync(await GetUserId());
            if(user == null)
            {
                return BadRequest(new ApiError(
                    errorMsg,
                    "User does not exist."));
            }

            if (assetId.Equals(user?.RootAssetId))
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
                    "The create timestamp cannot be later than the modify timestamp."));
            }

            return await AddOrUpdateResourceAsync(assetId, body, ct);
        }

        // DELETE /assets/{assetId}
        [Authorize]
        [HttpDelete("{assetId}", Name = nameof(DeleteAssetByIdAsync))]
        public async Task<IActionResult> DeleteAssetByIdAsync(
            string assetId,
            [FromQuery] DeletingOptions deletingOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var errorMsg = "Delete asset failed.";

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
                    deletingOptions.DelDependents,
                    ct);

                if(!result.Succeeded)
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

        private async Task<(bool IsValid, string Error)> CheckBaseForm(string userId, BaseAssetForm form, CancellationToken ct)
        {
            var category = await _categoryDataService.GetResourceAsync(userId, form.CategoryId, ct);
            if (category == null)
                return (false, "Category not found, please create category first.");

            var container = await _assetDataService.GetResourceAsync(userId, form.ContainerId, ct);
            if (container == null)
                return (false, "Container not found.");

            return (true, null);
        }

        #endregion
    }
}
